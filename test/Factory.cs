using System;
using System.Collections.Generic;

namespace test
{
    class Factory 
    {
        public void test1()
        {

            //Func<string, int> returnLength;
            //returnLength = text => text.Length;

            //Console.WriteLine(returnLength("Hello World!"));
        }


        
    }

    class Film
    {
        public string Name { get; set; }
        public int Year { get; set; }


        public void test()
        {
            var films = new List<Film>
            {
                new Film { Name = "Jaws",Year=1975},
                new Film { Name = "Singing in the Rain",Year = 1952},
                new Film { Name = "Some Like it Hot",Year = 1959},
                new Film { Name = "It's a Wonderful Life",Year = 1946},
                new Film { Name = "High Fidelity",Year = 2000},
                new Film { Name = "The Usual Suspects",Year = 1995}                
            };

            Action<Film> print =
                film => Console.WriteLine($"{film.Name},{film.Year}");

            films.ForEach(print);

            films.FindAll(film=>film.Year<1960).ForEach(print);

            films.Sort((f1,f2)=>f1.Name.CompareTo(f2.Name));
            films.ForEach(print);

        }
    }


    

}
