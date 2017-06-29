===============================================================================
SITES DE TEST
===============================================================================

	Les tests unitaires utilisent de "faux" sites internet pour permettre 
	la validation de GetFacts. Ces faux sites sont constitués de la sorte:

	www.site1.com

		Fichier template :	Templates\test\www.site1.com.json
		Cache html		 :	Cache\00000000-0000-0000-0000-000000000001.html
		
		Page très simple, contenant uniquement du texte. 
		Comprend 1 seule section, et trois articles.
		La section ne contient aucune information affichable.
		Les articles sont repérés par des 'id' uniques.

	www.site2.com

		Fichier template :	Templates\test\www.site2.com.json
		Cache html		 :	Cache\00000000-0000-0000-0000-000000000002.html
		
		Page très simple, contenant uniquement du texte. 
		Comprend 3 sections, dont une vide.
		Les articles sont repérés par des urls différents.