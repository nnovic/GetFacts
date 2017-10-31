using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace GetFactsTests
{
    static class FlowDocumentWalker
    {
        public delegate bool WalkerCallback(TextElement te);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="callback"></param>
        /// <returns>Le nombre de 'hits' positifs.</returns>
        public static uint Walk(FlowDocument doc, WalkerCallback callback)
        {
            uint hits = 0;
            foreach(Block b in doc.Blocks)
            {
                hits += Walk(b, callback);
            }
            return hits;
        }

        private static uint Walk(Block te, WalkerCallback callback)
        {
            uint hits = 0;

            if( te is Paragraph p)
            {
                foreach(Inline i in p.Inlines)
                {
                    hits += Walk(i, callback);
                }
            }
            else
            {
                throw new NotSupportedException();
            }

            return hits;
        }

        private static uint Walk(Inline i, WalkerCallback callback)
        {
            uint hits = 0;

            if( callback(i) )
            {
                hits++;
            }

            if( i is Span span)
            {
                foreach(Inline i2 in span.Inlines)
                {
                    hits += Walk(i2, callback);
                }
            }
            else if (i is Run)
            {
                // rien à faire.
            }
            else
            {
                throw new NotSupportedException();
            }

            return hits;
        }
    }
}
