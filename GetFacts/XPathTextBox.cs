using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace GetFacts
{
    public class XPathTextBox : TextBoxWithValidation
    {
        protected override bool IsTextValid(string text)
        {
            try
            {
                XPathExpression result = XPathExpression.Compile(text);
                return true;
            }
            catch //(Exception ex)
            {
                //MessageBox.Show("XPath syntax error: " + ex.Message);

            }
            return false;
        }
    }
}
