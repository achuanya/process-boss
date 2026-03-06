namespace ProcessBoss.Models
{
    public class ComboItem<T>
    {
        public T Value { get; set; }
        public string Display { get; set; }

        public override string ToString()
        {
            return Display;
        }
    }
}
