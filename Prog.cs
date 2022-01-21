using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace DS
{
    public class Drug
    {
        public double price = 0;
        public string name;
        public Drug(string name, double price)
        {
            this.name = name;
            this.price = price;
        }
    }
    public class Pharmacy
    {
        string characters = "abcdefghijklmnopqrstuvwxyz";
        public Random Rnd = new Random();
        public static long Encode_HashCode(string value)
        {
            long result = 0;
            for(int i = 0; i < value.Length; i++)
            {
                result += (result * 10 + (int)(value[i]) - 68);
            }
            return result;
        }
        public void Put_Drug_Into_Table(Drug drug)
        {
            long hashCode = Encode_HashCode(drug.name) % this.Drugs_Capacity;
            while(this.Drugs_With_Their_Interactions_Drug[hashCode].Key != null)
                hashCode = (hashCode + 1) % this.Drugs_Capacity;
            var kv = new KeyValuePair<Drug, Dictionary<string, string>>(drug, null);
            this.Drugs_With_Their_Interactions_Drug[hashCode] = kv;
        }
        public void Put_Disease_Into_Table(string disease)
        {
            long hashCode = Encode_HashCode(disease) % this.Disease_Capacity;
            while(this.Disease_With_Their_Effective_Drugs[hashCode].Key != null)
                hashCode = (hashCode + 1) % this.Disease_Capacity;
            var kv = new KeyValuePair<string, Dictionary<string, char>>(disease, null);
            this.Disease_With_Their_Effective_Drugs[hashCode] = kv;
        }
        public char createRnd_pos_neg_Effect() => (Rnd.Next() % 2 == 0) ? '+' : '-' ;
        int Drugs_number = 0;
        int Drugs_Capacity = 100_000;
        int Disease_number = 0;
        int Disease_Capacity = 100_000;
        public KeyValuePair<Drug, Dictionary<string, string>>[] Drugs_With_Their_Interactions_Drug;
        public KeyValuePair<string, Dictionary<string, char>>[] Disease_With_Their_Effective_Drugs;
        public Pharmacy()
        {
            this.Drugs_With_Their_Interactions_Drug = new KeyValuePair<Drug, Dictionary<string, string>>[this.Drugs_Capacity];
            this.Disease_With_Their_Effective_Drugs = new KeyValuePair<string, Dictionary<string, char>>[this.Disease_Capacity];

            #region Read Drug Data from dataset_1
                using (StreamReader sr = new StreamReader("./dataset_1.txt"))
                {
                    string line = sr.ReadLine();
                    while(line != null)
                    {
                        var a = line.Split(':').Select(x => x.Trim()).ToArray();
                        this.Create_Drug(a[0], double.Parse(a[1]));
                        line = sr.ReadLine();
                    }
                }
            #endregion

            #region Read Disease Data from dataset_2
                using (StreamReader sr = new StreamReader("./dataset_2.txt"))
                {
                    string line = sr.ReadLine();
                    while(line != null)
                    {
                        Create_Disease(line);
                        line = sr.ReadLine();
                    }
                }
            #endregion
        }

        public void Create_Drug(string name, double price, bool first_mode = true)
        {
            if(this.Drugs_number >= this.Drugs_Capacity)
            {
                var temp = this.Drugs_With_Their_Interactions_Drug.ToList();
                for(int _ = 0; _ < 10; _++)
                    temp.Add(new KeyValuePair<Drug, Dictionary<string,string>>(null, null));
                this.Drugs_With_Their_Interactions_Drug = temp.ToArray();
                temp = null;
                this.Drugs_Capacity += 10;
            }
            Drug d = new Drug(name, price);
            this.Put_Drug_Into_Table(d);
            this.Drugs_number ++;
            if(!first_mode)
            {
                int random_count = this.Rnd.Next(1, 9);
                var interactions_drugs = new string[random_count];
                int idx = 0;
                List<int> indexes = new List<int>();
                for(int _ = 0; _ < interactions_drugs.Length; _++)
                {
                    int i = this.Rnd.Next(this.Drugs_number);
                    while(indexes.Contains(i)||this.Drugs_With_Their_Interactions_Drug[i].Key.name == name)
                        i = this.Rnd.Next(this.Drugs_number);
                    interactions_drugs[idx] = this.Drugs_With_Their_Interactions_Drug[i].Key.name;
                    idx++;
                    indexes.Add(i);
                }
                indexes.Clear();
                idx = 0;
                random_count = this.Rnd.Next(1, 8);
                var Effective_diseases = new string[random_count];
                for(int _ = 0; _ < Effective_diseases.Length; _++)
                {
                    int i = this.Rnd.Next(this.Disease_number);
                    while(indexes.Contains(i))
                        i = this.Rnd.Next(this.Disease_number);
                    Effective_diseases[idx] = this.Disease_With_Their_Effective_Drugs[i].Key;
                    idx++;
                    indexes.Add(i);
                }

                string effect = "Eff_";
                for(int i = 0; i < interactions_drugs.Length; i++)
                {
                    for(int _ = 0; _ < 10; _++)
                        effect += this.characters[this.Rnd.Next(26)];
                    this.Make_Interaction(name, interactions_drugs[i], effect);
                    this.Make_Interaction(interactions_drugs[i], name, effect);
                    effect = "Eff_";
                }

                for(int i = 0; i<Effective_diseases.Length; i++)
                {
                    this.Make_Allergie(Effective_diseases[i], name, this.createRnd_pos_neg_Effect());
                }
            }
        }
        public void Create_Disease(string disease, bool first_mode = true)
        {
            if(this.Disease_number >= this.Disease_Capacity)
            {
                var temp = this.Disease_With_Their_Effective_Drugs.ToList();
                for(int _ = 0; _ < 10; _++)
                    temp.Add(new KeyValuePair<string, Dictionary<string, char>>(null, null));
                this.Disease_With_Their_Effective_Drugs = temp.ToArray();
                temp = null;
                this.Disease_Capacity += 10;
            }
            this.Put_Disease_Into_Table(disease);
            this.Disease_number ++;
            if(!first_mode)
            {
                int random_count = this.Rnd.Next(40);
                var Effective_drugs = new string[random_count];
                int idx = 0;
                for(int _ = 0; _ < Effective_drugs.Length; _++)
                {
                    int i = this.Rnd.Next(this.Drugs_number);
                    Effective_drugs[idx] = this.Drugs_With_Their_Interactions_Drug[i].Key.name;
                    idx++;
                }

                for(int i = 0; i<Effective_drugs.Length; i++)
                {
                    this.Make_Allergie(disease, Effective_drugs[i], this.createRnd_pos_neg_Effect());
                }
            }
        }
        public void Delete_Drug(string drug)
        {
            long hashCode = Encode_HashCode(drug) % this.Drugs_Capacity;
            while(this.Drugs_With_Their_Interactions_Drug[hashCode].Key==null||this.Drugs_With_Their_Interactions_Drug[hashCode].Key.name != drug)
            {
                hashCode = (hashCode + 1) % this.Drugs_Capacity;
            }
            var interactions_drugs = this.Drugs_With_Their_Interactions_Drug[hashCode].Value?.ToArray();
            this.Drugs_With_Their_Interactions_Drug[hashCode] = new KeyValuePair<Drug, Dictionary<string, string>>(null, null);
            this.Drugs_number--;
            for(int i = 0; i<interactions_drugs?.Length; i++)
            {
                hashCode = Encode_HashCode(interactions_drugs[i].Key) % this.Drugs_Capacity;
                while(this.Drugs_With_Their_Interactions_Drug[hashCode].Key==null||this.Drugs_With_Their_Interactions_Drug[hashCode].Key.name != interactions_drugs[i].Key)
                {
                    hashCode = (hashCode + 1) % this.Drugs_Capacity;
                }
                this.Drugs_With_Their_Interactions_Drug[hashCode].Value.Remove(drug);
            }
            for(int i = 0; i<this.Disease_With_Their_Effective_Drugs.Length; i++)
            {
                if(this.Disease_With_Their_Effective_Drugs[i].Value!=null && this.Disease_With_Their_Effective_Drugs[i].Value.ContainsKey(drug))
                {
                    this.Disease_With_Their_Effective_Drugs[i].Value.Remove(drug);
                }
            }
        }
        public void Delete_Disease(string disease)
        {
            long hashCode = Encode_HashCode(disease) % this.Disease_Capacity;
            while(this.Disease_With_Their_Effective_Drugs[hashCode].Key != disease)
                hashCode = (hashCode + 1) % this.Disease_Capacity;
            this.Disease_number--;
            this.Disease_With_Their_Effective_Drugs[hashCode] = new KeyValuePair<string, Dictionary<string, char>>(null, null);
        }

        public void Make_All_Interactions_Drug()
        {
            string[] line;
            using(StreamReader sr = new StreamReader("./dataset_3.txt"))
            {
                string l = sr.ReadLine();
                while(l != null)
                {
                    line = l.Split(':', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
                    var effected_drugs = line[1].Split(new char[]{';', ' '}, StringSplitOptions.RemoveEmptyEntries)
                                .Select(item => {
                                    var a = item.Split(new char[]{'(', ')', ','}, StringSplitOptions.RemoveEmptyEntries);
                                    return (drug:a[0], effect : a[1]);
                                }).ToArray();

                    for(int i = 0; i < effected_drugs.Length; i++)
                    {
                        this.Make_Interaction(line[0], effected_drugs[i].drug, effected_drugs[i].effect);
                    }
                    l = sr.ReadLine();
                }
            }
        }
        public void Make_All_Allergies()
        {
            string[] line;
            (string drug, char effect)[] drugs_with_effect;
            using(StreamReader sr = new StreamReader("./dataset_4.txt"))
            {
                string l = sr.ReadLine();
                while(l != null)
                {
                    line = l.Split(':').Select(item => item.Trim()).ToArray();
                    drugs_with_effect = line[1].Split(';').Select(item => { 
                                    var a = item.Split(new char[]{'(', ')', ',', ' '}, StringSplitOptions.RemoveEmptyEntries);
                                    return (drug : a[0], effect : a[1][0]);
                                }).ToArray();

                    for(int i = 0; i <drugs_with_effect.Length; i++)
                    {
                        Make_Allergie(line[0], drugs_with_effect[i].drug, drugs_with_effect[i].effect);
                    }

                    l = sr.ReadLine();
                }
            }
        }
        public void Make_Interaction(string d1, string d2, string effect)
        {
            long hashCode = Encode_HashCode(d1) % this.Drugs_Capacity;
            while(this.Drugs_With_Their_Interactions_Drug[hashCode].Key==null||this.Drugs_With_Their_Interactions_Drug[hashCode].Key.name != d1)
            {
                hashCode = (hashCode + 1) % this.Drugs_Capacity;
            }
            if(this.Drugs_With_Their_Interactions_Drug[hashCode].Value == null)
            {
                var t = new Dictionary<string, string>();
                this.Drugs_With_Their_Interactions_Drug[hashCode] = new KeyValuePair<Drug, Dictionary<string,string>>(this.Drugs_With_Their_Interactions_Drug[hashCode].Key, t);
            }
            this.Drugs_With_Their_Interactions_Drug[hashCode].Value.Add(d2, effect);
        }
        public void Make_Allergie(string disease, string drug, char allergy)
        {
            long hashCode = Encode_HashCode(disease) % this.Disease_Capacity;
            while(this.Disease_With_Their_Effective_Drugs[hashCode].Key != disease)
            {
                hashCode = (hashCode + 1) % this.Disease_Capacity;
            }
            if(this.Disease_With_Their_Effective_Drugs[hashCode].Value == null)
            {
                var t = new Dictionary<string, char>();
                this.Disease_With_Their_Effective_Drugs[hashCode] = new KeyValuePair<string, Dictionary<string, char>>(this.Disease_With_Their_Effective_Drugs[hashCode].Key, t);
            }
            this.Disease_With_Their_Effective_Drugs[hashCode].Value.Add(drug, allergy);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Pharmacy p = new Pharmacy();
            p.Make_All_Interactions_Drug();
            p.Make_All_Allergies();
            // p.Create_Drug("Drug_hvtiayzegu", 8645345, false);
            // p.Delete_Drug("Drug_hvtiayzegu");
            // p.Delete_Drug("Drug_twsvuusyqt");
            // p.Create_Disease("Dis_akfjdfaklj", false);
            // p.Delete_Disease("Dis_akfjdfaklj");
            while(true)
            {
                System.Console.WriteLine("1. start ");
                System.Console.WriteLine("2. Check for drug interactions in a prescription ");
                System.Console.WriteLine("3. Check for drug allergies in a prescription ");
                System.Console.WriteLine("4. Calculate the invoice price of the prescription ");
                System.Console.WriteLine("5. Changing the price of the drugs ");
                System.Console.WriteLine("6. Add or remove from dataset ");
                System.Console.WriteLine("7. Search ");
                string option;
                int option_number;
                while(true)
                {
                    try
                    {
                        System.Console.WriteLine("Choose Option(1-7)");
                        option = System.Console.ReadLine();
                        option_number = int.Parse(option);
                        if(option_number != 1)
                        {
                            throw new Exception("Invalid Input");
                        }
                        break;
                    }
                    catch (System.FormatException)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        System.Console.WriteLine("Your input isn't in correct format, please insert number");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    catch (Exception)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        System.Console.WriteLine("For accessing to drugs data insert 1 first of all");
                        Console.ForegroundColor = ConsoleColor.Red;
                        System.Console.WriteLine("Your input wasn't in correct range, please insert any number in range 1-7");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                switch (option_number)
                {
                    case 1:
                    break;

                    case 2:
                    break;

                    case 3:
                    break;

                    case 4:
                    break;

                    case 5:
                    break;

                    case 6:
                    break;

                    case 7:
                    break;
                }
            }
        }
    }
}
