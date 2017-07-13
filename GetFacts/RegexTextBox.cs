using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GetFacts
{
    public class RegexTextBox : TextBoxWithValidation
    {
        protected override bool IsTextValid(string text)
        {
            try
            {
                Regex result = new Regex(text);
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
