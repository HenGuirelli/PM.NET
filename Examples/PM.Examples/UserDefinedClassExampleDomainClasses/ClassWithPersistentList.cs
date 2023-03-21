using PM.Collections;

namespace PM.Examples.UserDefinedClassExampleDomainClasses
{
    public class ClassWithPersistentList
    {
        public virtual string PropString { get; set; } = string.Empty;
        public virtual PmList<ListItem> ItemList { get; set; }
    }

    public class ListItem
    {
        public virtual string Val { get; set; }
    }
}
