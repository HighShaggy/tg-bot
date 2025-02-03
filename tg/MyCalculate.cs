using System;
using System.Linq;

public class MyCalculate
{

    private string _inputMessage;

    public MyCalculate(string inputMessage)
    {
        _inputMessage = inputMessage;
    }

    public decimal[] Getkilometers() => _inputMessage.Split(' ').Select(decimal.Parse).ToArray();

    public decimal[] Calculete() => Getkilometers().Select(x => Math.Round(x * 8, 0)).ToArray();

}


