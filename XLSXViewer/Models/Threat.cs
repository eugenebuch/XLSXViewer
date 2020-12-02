using System;

namespace XLSXViewer.Models
{
    public class Threat : IEquatable<Threat>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public string Object { get; set; }
        public string Privacy { get; set; }
        public string Integrity { get; set; }
        public string Availabilty { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Threat);
        }

        public bool Equals(Threat other)
        {
            return other != null &&
                   Id == other.Id &&
                   Name == other.Name &&
                   Description == other.Description &&
                   Source == other.Source &&
                   Object == other.Object &&
                   Privacy == other.Privacy &&
                   Integrity == other.Integrity &&
                   Availabilty == other.Availabilty;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Description, Source, Object, Privacy, Integrity, Availabilty);
        }
    }
}
