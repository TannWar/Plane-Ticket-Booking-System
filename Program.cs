// // See https://aka.ms/new-console-template for more information
// Console.WriteLine("Hello, World!");
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Cryptography;

namespace _445project2 { }

delegate void priceEvent(float pr, Airline airline); // delegate defining

class Program
{
    public static Random rng = new Random();

    public static String key = "1234567891111111";
    //  public static Retailer r1 = new Retailer(1);
    //  public static Retailer r2 = new Retailer(2);
    public static BufferObject bo = new BufferObject();
    static void Main(string[] args)
    {
        Airline airline1 = new Airline(1);
        Airline airline2 = new Airline(2);

        TravelAgency travelAgency1 = new TravelAgency(1);
        TravelAgency travelAgency2 = new TravelAgency(2);

        airline1.promotion += new priceEvent(travelAgency1.agencyFunc);
        airline1.promotion += new priceEvent(travelAgency2.agencyFunc);
        airline2.promotion += new priceEvent(travelAgency1.agencyFunc);
        airline2.promotion += new priceEvent(travelAgency2.agencyFunc);

        Thread airlineThread = new Thread(new ThreadStart(airline1.airlineFunc));
        airlineThread.Name = "Airline 1";
        airlineThread.Start();
        Thread airlineThread2 = new Thread(new ThreadStart(airline2.airlineFunc));
        airlineThread2.Name = "Airline 2";
        airlineThread2.Start();
        // PRICING MODEL HIGH LEVEL
        Thread priceChange = new Thread(delegate () {
            float price = 5000;

            while (true)
            {
                price = price + Program.rng.Next(-250, 100);
                airline1.priceChange(price);
                Thread.Sleep(500);
            }
        });
        priceChange.Start();

        Thread priceChange2 = new Thread(delegate () {
            float price = 5000;

            while (true)
            {
                price = price + Program.rng.Next(-250, 100);
                airline2.priceChange(price);
                Thread.Sleep(500);
            }
        });
        priceChange2.Start();

        airlineThread.Join();
        airlineThread2.Join();

        Console.WriteLine("");
        Console.WriteLine("Summary: ");
        Console.WriteLine("There were a total of " + (airline1.numOfPriceChanges + airline2.numOfPriceChanges) + " price cut events");
        Console.WriteLine("Travel agencies placed a " + "total of " + (travelAgency1.orders + travelAgency2.orders) + " orders.");
        Console.WriteLine("Airlines successfully processed " + (airline1.numOfOrders + airline2.numOfOrders) + " orders.");
        Console.WriteLine(airline1.orderRejected + airline2.orderRejected + " orders were rejected.");

        //Console.WriteLine("Done");
        Console.ReadKey();
    }
}

class Airline
{
    public mCellBuffer cellBuffer = new mCellBuffer(); //order put into buffer

    public event priceEvent promotion;

    public int airlineID;

    public int numOfOrders;

    public int numOfPriceChanges;

    public float lowestPrice;

    public bool stillRun;

    public int orderRejected;

    public Airline(int ID)
    {
        numOfOrders = 0;
        numOfPriceChanges = 0;
        lowestPrice = 999999;
        stillRun = true;
        this.airlineID = ID;
    }

    public void priceChange(float price)
    {
        if (!stillRun)
        {
            return;
        }

        Console.WriteLine("Airline " + airlineID + " has gotten a price change, new price: " + price + " .");

        if (price < lowestPrice)
        {
            if (promotion != null)
            {
                promotion(price, this);
            }
            lowestPrice = price;
            numOfPriceChanges++;
        }
    }

    public void airlineFunc()
    {
        while (numOfPriceChanges < 10)
        {
            string getEncodedOrder = cellBuffer.Get();
            string getDecryptedOrder = encorderDecorder.Decrypt(getEncodedOrder, Program.key);

            OrderObject order = OrderObject.fromString(getDecryptedOrder);

            numOfOrders++;

            if (order.CardNumber.Length != 16)
            {
                orderRejected++;
                Console.WriteLine("Airline " + airlineID + " rejected an order from " + order.SenderID + " due to invalid card number.");
            }
            else
            {
                Console.WriteLine("Airline " + airlineID + " is processing an order from Travel agent " + order.SenderID + ".");
            }
        }
        stillRun = false;
    }
}

class TravelAgency
{
    private int senderID;

    public int orders;

    public TravelAgency(int senderID)
    {
        this.senderID = senderID;
    }

    public string randCardNumber()
    {
        if (Program.rng.Next(0, 2) == 0)
        {
            return "1234567891012345";
        }
        else
        {
            return "1";
        }
    }


    public void agencyFunc(float price, Airline airline)
    {
        int tickets = Program.rng.Next(100) + 1;
        float tax = 1.10F;
        float total = price * tickets * tax;

        OrderObject order = new OrderObject(senderID, price, 0, randCardNumber());
        string encrypt = encorderDecorder.Encrypt(order.getorder(), Program.key);

        orders++;
        Console.WriteLine("Travel Agenecy" + senderID + " has created an order at " + DateTime.Now.ToString());
        airline.cellBuffer.Put(encrypt);

        Console.WriteLine("Travel Agency " + senderID + " placed an order for " + tickets + " tickets, at price $" + price + " per seat for a total amount of $" + total + " after 10% tax is placed.");
    }
}


class OrderObject
{
    private int senderID;
    private float amount;
    private int receiverID;
    private string cardNumber; //later
    //public int orderRejected;

    public OrderObject(int senderID, float amount, int receiverID, string cardNumber/* int orderRejected */)
    {
        this.senderID = senderID;
        this.amount = amount;
        this.receiverID = receiverID;
        this.cardNumber = cardNumber;
        // this.orderRejected = orderRejected;

    }

    public int SenderID
    {
        get { return senderID; }
    }

    public float Amount
    {
        get { return amount; }
    }

    public int ReceiverID
    {
        get { return receiverID; }
    }

    public string CardNumber
    {
        get { return cardNumber; }
    }


    public static OrderObject fromString(string s)
    {
        int senderID = 0;
        float amount = 0;
        int receiverID = 0;
        string cardNumber = "";
        // int orderRejected = 0;

        string[] split = s.Split(' ');

        foreach (var tmp in split)
        {
            string[] newSplit = tmp.Split(':');

            if (newSplit[0].Equals("sender"))
            {
                senderID = int.Parse(newSplit[1]);
            }
            if (newSplit[0].Equals("amount"))
            {
                amount = float.Parse(newSplit[1]);
            }
            if (newSplit[0].Equals("receiver"))
            {
                receiverID = int.Parse(newSplit[1]);
            }
            if (newSplit[0].Equals("cardnumber"))
            {
                cardNumber = newSplit[1];
            }
        }
        return new OrderObject(senderID, amount, receiverID, cardNumber); //take string and recreate order object from it
    }

    public String getorder()
    {
        return "sender:" + senderID + " amount:" + amount + " receiver:" + receiverID + " cardnumber:" + cardNumber;
    }
}

class BufferObject
{
    private OrderObject o;
    public delegate void orderDelegate();
    public event orderDelegate makingOrder;
    bool writable = true;
    public BufferObject()
    {
        o = null;
    }

    public void setBuffer(OrderObject neworder)
    {

        while (!writable)
        {
            try
            {
                Monitor.Wait(this);
            }
            catch { }
        }
        o = neworder;
        writable = false;
        makingOrder();
        Monitor.PulseAll(this);
    }

    public String getBuffer()
    {

        while (writable)
        {
            try
            {
                Monitor.Wait(this);
            }
            catch { }
        }
        writable = true;
        Monitor.PulseAll(this);
        return o.getorder();
    }
}

class mCellBuffer
{
    Semaphore semaphorePut = new Semaphore(initialCount: 3, maximumCount: 3);
    Semaphore semaphoreGet = new Semaphore(initialCount: 0, maximumCount: 3);

    const int SIZE = 3;
    string[] data = new string[SIZE];
    int head = 0, tail = 0, n = 0;
    private Object BufferLock = new Object();

    public void Put(string c)
    {
        semaphorePut.WaitOne();
        lock (BufferLock)
        {
            // Console.WriteLine("writing thread entered");
            data[tail] = c;
            tail = (tail + 1) % SIZE;
            n++;
            semaphoreGet.Release();

            //Console.WriteLine("writing thread " + Thread.CurrentThread.Name + " " + c + " " + n);

            // Monitor.Pulse(BufferLock);

        }
    }

    public string Get()
    {
        semaphoreGet.WaitOne();
        lock (BufferLock)
        {
            // Console.WriteLine("reading thread entered");
            string c = data[head];
            head = (head + 1) % SIZE;
            n--;

            // Console.WriteLine("Reading thread " + Thread.CurrentThread.Name + " " + c + " " + n);
            // Monitor.Pulse(BufferLock);
            semaphorePut.Release();
            return c;


        }
    }
}


class encorderDecorder
{
    public delegate void encodeDelegate();
    public static event encodeDelegate encoded;


    //This method will be called when a retailer thread place an order
    public static void getOrder()
    {
        Monitor.Enter(Program.bo);
        try
        {
            string order = Program.bo.getBuffer();// get the order from the buffer
                                                  // Console.WriteLine("Got the order : " + order);
                                                  // Encript the order and send the order to two retailers r1 and r2
                                                  //  Program.r1.getEncoded(Encrypt(order, "ABCDEFGHIJKLMNOP"));
                                                  // Program.r2.getEncoded(Encrypt(order, "ABCDEFGHIJKLMNOP"));
        }
        finally { Monitor.Exit(Program.bo); }
    }
    public static string Encrypt(String input, String key)
    {
        byte[] inputArray = UTF8Encoding.UTF8.GetBytes(input);
        String result = "";
        try
        {
            TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
            tripleDES.Key = UTF8Encoding.UTF8.GetBytes(key);
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tripleDES.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            tripleDES.Clear();
            result = Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        catch (CryptographicException e)
        {
            Console.WriteLine(e.ToString());
        }
        return result;
    }

    public static String Decrypt(String input, string key)
    {
        Byte[] inputArray = Convert.FromBase64String(input);
        TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
        tripleDES.Key = UTF8Encoding.UTF8.GetBytes(key);
        tripleDES.Mode = CipherMode.ECB;
        tripleDES.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = tripleDES.CreateDecryptor();
        Byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
        tripleDES.Clear();
        return UTF8Encoding.UTF8.GetString(resultArray);
    }
}