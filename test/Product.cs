//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace test
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    public class Product
//    {
//        public Product(string name, decimal price)
//        {
//            Name = name;
//            Price = price;
//        }

        

//        public string Name { get; private set; }

        
//        public decimal Price { get; private set; }
            
//        public Product() { }

//        public static List<Product> GetSampleProducts()
//        {
//            return new List<Product>
//            {
//                new Product("West Side Story",9.99m),
//                new Product("Assassins",14.99m),
//                new Product("Frogs",13.99m),
//                new Product("Sweeney Todd",10.99m)                            
//            };
//        }

//        public override string ToString()
//        {
//            return string.Format("{0}:{1}", Name, Price);
//        }


//    }

//    public class ProductNameComparer : IComparer<Product>
//    {
//        public int Compare(Product x,Product y)
//        {
//            return x.Name.CompareTo(y.Name);
//        }


//        public int CompareTo(Product other)
//        {
//            throw new NotImplementedException();
//        }



//    }

//    class Test1
//    {
//        static void test()
//        {
//            List<Product> products = Product.GetSampleProducts();

//            //products.Sort(new ProductNameComparer());

//            products.Sort(delegate(Product x, Product y) { return x.Name.CompareTo(y.Name); });

//            Console.WriteLine("delegate output");
//            foreach (var product in products)
//            {
//                Console.WriteLine(product);
//            }


//            products.Sort((x,y)=>x.Price.CompareTo(y.Price));


//            Console.WriteLine("lambda output");
//            foreach (var product in products)
//            {
//                Console.WriteLine(product);
//            }


//            Predicate<Product> predicate =  p => p.Price > 10m;
//            List<Product> matchs = products.FindAll(predicate);

//            Action<Product> print = Console.WriteLine;
//            matchs.ForEach(print);


//            Console.WriteLine("Linq");
//            var filtered = from p in products
//                           where p.Price > 11 
//                           select p;
//            foreach (var product in filtered)
//            {
//                Console.WriteLine(product);
//            }



//        }
//    }






//}
