using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.NH2300
{
    public class Master
    {
        public virtual string Id { get; set; }
        public virtual string Name { get; set; }
        private IList<Detail> details = new List<Detail>();
        public virtual IList<Detail> Details
        {
            get { return details; }
            set { details = value; }
        }
    }


    public class Detail
    {
        public virtual string Id { get; set; }
        public virtual int Version { get; set; }
        public virtual string Description { get; set; }
        public virtual Master Parent { get; set; }
    }

}
