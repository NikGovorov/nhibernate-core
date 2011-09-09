using NHibernate.Cache;
using NHibernate.Cfg;
using NHibernate.Engine;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2300
{
    [TestFixture]
    public class Fixture : BugTestCase
    {
        protected override void OnSetUp()
        {
            base.OnSetUp();
            cfg.Properties[Environment.CacheProvider] = typeof (HashtableCacheProvider).AssemblyQualifiedName;
            cfg.Properties[Environment.UseQueryCache] = "true";
            sessions = (ISessionFactoryImplementor) cfg.BuildSessionFactory();

            using (var session = OpenSession())
            {
                using (var tran = session.BeginTransaction())
                {
                    var masterOne = new Master {Id = "mOne", Name = "Master One"};
                    masterOne.Details.Add(new Detail {Id = "dOne", Description = "Detail One", Parent = masterOne});
                    masterOne.Details.Add(new Detail {Id = "dTwo", Description = "Detail Two", Parent = masterOne});
                    session.Save(masterOne);
                    tran.Commit();
                }
            }
        }

        protected override void OnTearDown()
        {
            using (var session = OpenSession())
            {
                using (var tran = session.BeginTransaction())
                {
                    session.Delete("from Master");
                    tran.Commit();
                }
            }
            base.OnTearDown();
        }


        [Test]
        public void VersionShouldBeIncrementedIfEntityIsDirty()
        {
            using (var session = OpenSession())
            {
                using (var tran = session.BeginTransaction())
                {
                    var master = session.Get<Master>("mOne");
                    master.Details[0].Parent.Name += " Edited";
                    master.Details.RemoveAt(0);
                    tran.Commit();
                }
            }

            using (var session = OpenSession())
            {
                using (var tran = session.BeginTransaction())
                {
                    var master = session.Get<Master>("mOne");
                    Assert.AreEqual(2,
                                    (int) session.GetSessionImplementation().PersistenceContext.GetEntry(master).Version);
                    tran.Commit();
                }
            }
        }


        [Test]
        public void VersionShouldBeIncrementedIfLockWithForceOptionWasCalled()
        {
            using (var session = OpenSession())
            {
                using (var tran = session.BeginTransaction())
                {
                    var master = session.Get<Master>("mOne");
                    session.Lock(master, LockMode.Force);
                    tran.Commit();
                }
            }

            using (var session = OpenSession())
            {
                using (var tran = session.BeginTransaction())
                {
                    var master = session.Get<Master>("mOne");
                    Assert.AreEqual(2,
                                    (int) session.GetSessionImplementation().PersistenceContext.GetEntry(master).Version);
                    tran.Commit();
                }
            }
        }


        [Test]
        public void VersionShouldNotBeIncrementedIfNothingHappened()
        {
            using (var session = OpenSession())
            {
                using (var tran = session.BeginTransaction())
                {
                    var master = session.Get<Master>("mOne");
                    master.Details[0].Description += " Edited";
                    tran.Commit();
                }
            }

            using (var session = OpenSession())
            {
                using (var tran = session.BeginTransaction())
                {
                    var master = session.Get<Master>("mOne");
                    Assert.AreEqual(1,
                                    (int) session.GetSessionImplementation().PersistenceContext.GetEntry(master).Version);
                    tran.Commit();
                }
            }
        }
    }
}