using System;
using System.Collections.Generic;
using System.Text;

namespace Jtwor.ORM.Attributes
{
    public class Column : Attribute
    {
        private string _name { get; set; }

        public Column(string name)
        {
            this._name = name;
        }

        public string GetName() => _name;
    }
}
