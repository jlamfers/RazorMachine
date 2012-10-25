using System.Collections.Generic;

namespace Xipton.Razor.Example.Models {
    public class MyProduct
    {
        public string Name { get; set; }
        public double Price { get; set; }
    }

    public class MyProductList : List<MyProduct> {
        public MyProductList()
        {
                 Add("Toyota    ", 20.245)
                .Add("Opel      ", 12.938)
                .Add("BMW       ", 24.837)
                .Add("Skoda     ", 19.298);
        }

        private  MyProductList Add(string name, double price){
            Add(new MyProduct{Name = name, Price = price});
            return this;
        }
    }
}
