//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace test
//{

//    /// <summary>
//    /// MenuItem
//    /// </summary>
//    public class MenuItem
//    {
//        string name;
//        string description;
//        bool vegetarain;
//        double price;

//        /// <summary>
//        /// MenuItem
//        /// </summary>
//        /// <param name="name"></param>
//        /// <param name="description"></param>
//        /// <param name="vegetarian"></param>
//        /// <param name="price"></param>
//        public MenuItem(string name, string description, bool vegetarian, double price)
//        {
//            this.name = name;
//            this.description = description;
//            this.vegetarain = vegetarian;
//            this.price = price;
//        }

//        /// <summary>
//        /// getName
//        /// </summary>
//        /// <returns></returns>
//        public string getName()
//        {
//            return name;
//        }

//        /// <summary>
//        /// getDescription
//        /// </summary>
//        /// <returns></returns>
//        public string getDescription()
//        {
//            return description;
//        }

//        /// <summary>
//        /// getVegetarain
//        /// </summary>
//        /// <returns></returns>
//        public bool getVegetarain()
//        {
//            return vegetarain;
//        }

//        /// <summary>
//        /// getPrice
//        /// </summary>
//        /// <returns></returns>
//        public double getPrice()
//        {
//            return price;
//        }


//    }

//    /// <summary>
//    /// Iterator
//    /// </summary>
//    public interface Iterator
//    {
//        /// <summary>
//        /// hasNext
//        /// </summary>
//        /// <returns></returns>
//        bool hasNext();
//        /// <summary>
//        /// next
//        /// </summary>
//        /// <returns></returns>
//        Object next();

//    }
//    /// <summary>
//    /// Class DinerMenuIterator.
//    /// </summary>
//    /// <seealso cref="test.Iterator" />
//    public class DinerMenuIterator : Iterator
//    {
//        ArrayList items1 = new ArrayList();
        

//        MenuItem[] items;
//        int position = 0;
//        /// <summary>
//        /// Initializes a new instance of the <see cref="DinerMenuIterator"/> class.
//        /// </summary>
//        /// <param name="items">The items.</param>
//        public DinerMenuIterator(MenuItem[] items)
//        {
//            this.items = items;
            

//        }

//        /// <summary>
//        /// next
//        /// </summary>
//        /// <returns></returns>
//        public Object next()
//        {
//            MenuItem menuItem = items[position];
//            position = position + 1;
//            return menuItem;
//        }


//        /// <summary>
//        /// hasNext
//        /// </summary>
//        /// <returns></returns>
//        public bool hasNext()
//        {
//            return position < items.Length && items[position] != null;
//        }


//    }

//    /// <summary>
//    /// Class DinerMenu.
//    /// </summary>
//    public class DinerMenu
//    {
//        const int MAX_ITEMS = 6;
//        int numberOfItems = 0;
//        MenuItem[] menuItems;
//        /// <summary>
//        /// Creates the iterator.
//        /// </summary>
//        /// <returns>Iterator.</returns>
//        public Iterator CreateIterator()
//        {
//            return new DinerMenuIterator(menuItems);
//        }
//    }


//}
