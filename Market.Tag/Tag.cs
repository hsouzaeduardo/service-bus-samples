namespace Market.Tag
{
    public class Tag
    {
        public string Id { get; set; }

        public string Produto { get; set; }

        public double Preco { get; set; }

        public Tag()
        {
            Id = Guid.NewGuid().ToString();
        }

        public override string ToString()
        {
            return $"Produto:{ Produto }\tPreco:${ Preco }";
        }
    }
}