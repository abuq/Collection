//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.Remoting.Messaging;
//using System.Text;

//namespace test
//{
//    class DelegateTest
//    {
//        delegate void StringProcessor(string input);

//        class Person
//        {
//            string name;

//            public Person(string name)
//            {
//                this.name = name;
//            }

//            public void Say(string message)
//            {
//                Console.WriteLine("{0} Says:{1}", name, message);
//            }
//        }

//        class Background
//        {
//            public static void Note(string note)
//            {
//                Console.WriteLine("({0})", note);
//            }
//        }

//        class SimpleDelegateUse
//        {
//            /// <summary>
//            /// Defines the entry point of the application.
//            /// </summary>
//            //private static void Main()
//            //{
//            //    Person jon = new Person("jon");
//            //    Person tom = new Person("tom");
//            //    StringProcessor jonsVoice, tomsVoice, background;
//            //    jonsVoice = new StringProcessor(jon.Say);
//            //    tomsVoice = new StringProcessor(tom.Say);
//            //    background = new StringProcessor(Background.Note);

//            //    jonsVoice("Hello,son.");
//            //    tomsVoice("Hello, Daddy!");
//            //    background("An airplane flies past.");


//            //    Action<string> action = s => Console.WriteLine(s);
//            //    Action<string> action1 = delegate(string s) {Console.WriteLine(s);  };

//            //    action("action");
//            //    action1("action1");

//            //    Predicate<string> predicate = s => s.Equals("predicate");
//            //    Predicate<string> predicate1 = delegate(string s)
//            //    {
//            //        return s.Equals("predicate1");
//            //    };
//            //    Console.WriteLine(predicate("predicate"));
//            //    Console.WriteLine(predicate1("predicate1"));

//            //    Func<string,string> func = s => s+1;
//            //    Func<string, string> func1 = delegate(string s)
//            //    {
//            //        return s + 1;
//            //    };

//            //    var two = func("1");
//            //    var three = func("2");
//            //    Console.WriteLine(two);
//            //    Console.WriteLine(three);

//            //}
//        }
//    }
//}
