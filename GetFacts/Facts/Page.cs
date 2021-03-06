﻿using GetFacts.Download;
using GetFacts.Parse;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace GetFacts.Facts
{
    [DebuggerDisplay("Page = {PageName}")]
    public class Page:AbstractInfo
    {
        private readonly string pageUrl;
        private readonly Section defaultSection;

        public Page(string url)
        {
            pageUrl = url;
            BaseUri = new Uri(url, UriKind.Absolute);
            defaultSection = new Section(String.Empty);
            defaultSection.BaseUri = this.BaseUri;
            RefreshDelay = 10;
        }

        public Page(PageConfig pc):this(pc.Url)
        {            
            Template = TemplateFactory.GetInstance().GetExistingTemplate(pc.Template);
            Parser = AbstractParser.NewInstance(Template.PageType);
            RefreshDelay = pc.Refresh * 60; // convertir les minutes en secondes
            Enabled = pc.Enabled;
            IsNewBehavior = pc.IsNewBehavior;

            if ( string.IsNullOrEmpty(pc.Name)==false )
            {
                PageName = pc.Name;
            }
            else
            {
                PageName = Template.PageName;
            }
        }

        /// <summary>
        /// Destruction de cet objet:
        /// - s'assurer que toutes les notifications poussées dans
        ///   NotificationSystem par cet objet soient supprimées.
        /// </summary>
        ~Page()
        {
            NotificationSystem.GetInstance().RemoveAll(this);
        }

        /// <summary>
        /// Utilisé pour les tests automatiques, permet d'obtenir
        /// la référence sur l'objet Section qui représente
        /// la section par défaut de cette Page.
        /// </summary>
        internal Section DefaultSection => defaultSection;

        /// <summary>
        /// Retourne l'appellation qui a été
        /// donnée à cette page par l'utilisateur.
        /// </summary>
        /// <remarks>
        /// Il s'agit de la valeur de PageConfig.Name, ou
        /// par défaut de Template.PageName.
        /// </remarks>
        /// <seealso cref="PageConfig.Name"/>
        /// <seealso cref="PageTemplate.PageName"/>
        public string PageName
        {
            get;
            private set;
        }

        /// <summary>
        /// delai (en secondes) entre le moment
        /// où le téléchargement du document est terminé
        /// avec succès
        /// et le moment où il va recommencer.
        /// </summary>
        internal int RefreshDelay
        {
            get;
            set;
        }

        internal bool Enabled
        {
            get;
            set;
        }

        /// <summary>
        /// délai (en secondes) entre le moment
        /// où le téléchargement à échoué et celuis
        /// où l'on va le retenter.
        /// Utilisé aussi pour le temporiser le
        /// tout premier téléchargé de la page, au
        /// démarrage du programme.
        /// </summary>
        internal int RecoverDelay
        {
            get { return 10; }
        }

        #region configuration

        public AbstractParser Parser
        {
            get;set;
        }

        /// <summary>
        /// Retourne l'extension de fichier qui est
        /// la plus communément utilisée pour pour
        /// le type de fichier qui sert de source à
        /// cette page.
        /// Peut retourne null si cette information
        /// n'est pas disponible.
        /// </summary>
        /// <example>Pour des données au format HTML, DefaultFileExtension
        /// retournera ".html"</example>
        /// <remarks>Retourne le tout premier élément de Parser.UsualFileExtensions</remarks>
        public string DefaultFileExtension
        {
            get
            {
                return Parser.MostProbableFileExtension;
            }
        }

        public string Url
        {
            get
            {
                return pageUrl;
            }
        }

        public PageTemplate Template
        {
            get;set;
        }

        #endregion

        #region mise à jour 

        /// <summary>
        /// Liste des clés pour créer/supprimer des
        /// notifications dans NotificationSystem.
        /// </summary>
        /// <seealso cref="NotificationSystem"/>
        enum NotificationKeys
        {
            /// <summary>
            /// Une notification sera ajoutée si, à l'issue
            /// de la mise à jour de la page, aucun article
            /// a été trouvé.
            /// </summary>
            /// <seealso cref="HasAnyArticle"/>
            NoArticleInTheEntirePage
        }

        public void Update(string sourceFile)
        {            
            Parser.Load(sourceFile, Template.Encoding);
            Update(Parser.CreateNavigator());
        }

        private void Update(XPathNavigator nav)
        {            
            BeginUpdate();
            
            UpdateInfo(nav, Template);

            // Si jamais la page n'a pas de titre à l'issu
            // de l'analyse, on propose le nom 
            if( string.IsNullOrEmpty(this.Title) )
            {
                this.Title = PageName;
            }

            foreach (SectionTemplate sectionTemplate in Template.Sections)
            {
                try
                {
                    foreach (XPathNavigator subTree in nav.Select(sectionTemplate.XPathFilter))
                    {
                        if (subTree == null)
                            continue;

                        string configName = sectionTemplate.SectionName;
                        string title = sectionTemplate.TitleTemplate.Execute(subTree);
                        string text = sectionTemplate.TextTemplate.Execute(subTree);
                        Section s = FindSection(configName, title, text);

                        if (s != null)
                        {
                            s.Update(subTree, sectionTemplate);
                        }
                    }
                }
                catch
                {
                    // ne pas bloquer la mise à jour des sections
                    // suivantes si une erreur s'est produite.
                }
            }

            EndUpdate();

            var notification = new NotificationSystem.Notification(this, 
                (int)NotificationKeys.NoArticleInTheEntirePage)
            {
                Title = PageName,
                Description = "No article has been found."
            };

            if( HasAnyArticle )
            {
                NotificationSystem.GetInstance().Remove(notification);
            }
            else
            {
                NotificationSystem.GetInstance().Add(notification);
            }
        }        

        #endregion

        #region Sections

        public int SectionsCount
        {
            get
            {
                int count = Children.Count;
                if (count == 0) return 1; // section par défaut, présente si aucune autre section n'est définie.
                else return count;
            }
        }

        public bool HasAnyArticle
        {
            get
            {
                for(int i=0;i<SectionsCount;i++)
                {
                    if (GetSection(i).ArticlesCount > 0)
                        return true;
                }
                return false;
            }

        }

        /// <summary>
        /// Utilisée uniquement pour faire des tests,
        /// cette méthode ajoute une nouvelle section à la page.
        /// Les caractéristiques de cette nouvelle section sont spécifiés
        /// par les paramètres de la méthode.
        /// </summary>
        /// <remarks>
        /// Utilisée uniquement pour faire des tests,
        /// aucune vérification sur la validité des paramètres 
        /// n'est effectuée.
        /// </remarks>
        internal Section AddSection(string name, string title, string text)
        {
            Section s = new Section(name)
            {
                Title=title,
                Text=text
            };
            Children.Add(s);
            return s;
        }

        /// <summary>
        /// Retourne l'instance de Section qui est stocké dans la liste
        /// à l'index donné en argument.
        /// </summary>
        /// <param name="index">Index (à partir de zéro) de l'objet recherché.</param>
        /// <returns>Instance de Section stocké à l'index spécifié.</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public Section GetSection(int index)
        {
            if (Children.Count == 0)
            {
                if (index == 0)
                    return defaultSection;
                else
                    throw new IndexOutOfRangeException();
            }
            else
            {
                return (Section)Children[index];
            }
        }

        /// <summary>
        /// Permet d'obtenir l'objet Section portant le nom spécifié en paramètre.
        /// Si aucun nom n'est spécifié (chaine nulle ou vide), une instance par défaut est retournée.
        /// Si aucune section n'existe avec le nom spécifié, une nouvelle section est créée implicitement avec ce nom.
        /// </summary>
        /// <param name="name"></param>
        /// <remarks>Lors de la création implicite d'une nouvelle section portant le nom passé en argument, les
        /// champs BaseUri et IsNewBehavior de la section sont initialisés avec les valeurs des propriétés
        /// homonymes de la Page en cours.</remarks>
        public Section FindSection(string name, string title, string text)
        {
            if( string.IsNullOrEmpty(name) )
            {
                return defaultSection;
            }


            var strictSelection = from child in Children
                           where (child.Identifier == name)
                           && (child.Title == title)
                                && (child.Text == text)
                           select child;

            var looseSelection = from child in Children
                           where (child.Identifier == name) 
                           && ((string.IsNullOrEmpty(title)==false && child.Title == title) 
                                || (string.IsNullOrEmpty(text)==false && child.Text == text))
                           select child;
            

            if(strictSelection.Count()==1)
            {
                return (Section)strictSelection.Single();
            }

            else if (looseSelection.Count() == 1)
            {
                return (Section)looseSelection.Single();
            }

            else
            {
                Section output = new Section(name)
                {
                    BaseUri = this.BaseUri,
                    IsNewBehavior = this.IsNewBehavior,
                    Title = title,
                    Text = text
                };
                Children.Add(output);
                return output;
            }
        }

        #endregion
    }
}
