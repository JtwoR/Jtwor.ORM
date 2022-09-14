using System;
using System.Collections.Generic;
using System.Text;

namespace Jtwor.ORM.Attributes
{
    public class Table : Attribute
    {
        private string _name { get; set; }

        public Table(string name) {
            this._name = name;
        }

        public string GetName() => _name;
    }
}
