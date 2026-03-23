using Budget_Buddy.Models;
using Java.Time;
using Microcharts;
using Microcharts.Maui;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace Budget_Buddy;

public partial class ChartBreakdown : ContentPage
{
    double MonthlyIncome = 0;
    double BillTotal = 0;
	public ChartBreakdown(double monthlyIncome, double monthlyBillTotal)
	{
        MonthlyIncome = monthlyIncome;
        BillTotal = monthlyBillTotal;
        Console.WriteLine(BillTotal);

		InitializeComponent();
        
	}

    async void ContentPage_Loaded(object sender, EventArgs e)
    {
        monthly_bills_total_label.Text = $"${BillTotal.ToString()}";
        await MakePieChart();

    }

    private async Task MakePieChart()
    {
        string[] colors = { "#1f3f5c", "#2670b5", "#5999d4", "#0c2d4d", "#2aa4bd", "#007d96", "#12414a", "#7c61ab", "#462a75", "#1a0440" };
        await DBHandler.GetCategories(Dashboard.UserID);

        ChartEntry[] entries = new ChartEntry[Category.AllCategories.Count + 1];

        Dictionary<string, double> CategoryPrices = new Dictionary<string, double>();

        double totalBillAmount = 0;

        for (int i = 0; i < Category.AllCategories.Count; i++)
        {
            CategoryPrices.Add(Category.AllCategories[i].Name, 0);
        }

        CategoryPrices.Add("Other", 0);

        foreach (Bill bill in Bill.BillList)
        {
            try { CategoryPrices[bill.Category] += bill.Price; totalBillAmount += bill.Price; BillTotal += bill.Price; }
            catch (Exception ex)
            {
                CategoryPrices["Other"] += bill.Price;
                totalBillAmount += bill.Price;
            }
        }

        foreach (Bill bill in Bill.RecurringBillList)
        {
            try { CategoryPrices[bill.Category] += bill.Price; totalBillAmount += bill.Price; BillTotal += bill.Price; }
            catch (Exception ex)
            {
                CategoryPrices["Other"] += bill.Price;
                totalBillAmount += bill.Price;
            }
        }

        foreach (Bill bill in Bill.TempBillList)
        {
            try { CategoryPrices[bill.Category] += bill.Price; totalBillAmount += bill.Price; BillTotal += bill.Price; }
            catch (Exception ex)
            {
                CategoryPrices["Other"] += bill.Price;
                totalBillAmount += bill.Price;
            }
        }

        //Assign Colors Based on Dark mode / light mode
        string LabelColorString = "#404040";
        string ValueColorString = "#141414";
        string BackgroundColorString = "#FFFFFF";
        if (Application.Current.RequestedTheme == AppTheme.Dark)
        {
            ValueColorString = "#E1E1E1";
            LabelColorString = "#def1ff";
            BackgroundColorString = "#1f1f1f";
        }

        int j = 0;
        foreach (var entry in CategoryPrices)
        {
            entries[j] = new ChartEntry((float)entry.Value) { Label = entry.Key, Color = SKColor.Parse(colors[j]), ValueLabelColor = SKColor.Parse(ValueColorString), ValueLabel = $"${entry.Value} ({Math.Round((entry.Value / MonthlyIncome) * 100, 0)}%)" };
            j++;
        }

        chartView.Chart = new PieChart
        {
            Entries = entries,
            LabelTextSize = 40,
            LabelMode = LabelMode.RightOnly,
            LabelColor = SKColor.Parse(LabelColorString),
            AnimationDuration = TimeSpan.FromSeconds(3.5),
            BackgroundColor = SKColor.Parse(BackgroundColorString)
        };


    }

    private void current_balance_entry_textchanged(object sender, TextChangedEventArgs e)
    {

    }
}