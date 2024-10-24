using ConsoleApp1.Core.Expressions.Atoms;

namespace ConsoleApp1.Core.NumericalAnalysis;

public class Regression
{

    public const int MAX_EPOCHS = 1000;
    
    public static void RegressionFor(Expr f, Variable x, Variable[] parametres, (double x, double y)[] points, double lr = 0.01, int epochs = MAX_EPOCHS)
    {
        if (points.Length == 0)
            throw new ArgumentException("There are no points to calculate the regression");

        var dw = new Expr[parametres.Length];
        
        for (int i = 0; i < parametres.Length; i++)
        {
            if (parametres[i].Value is not Number)
                parametres[i].Value = 0;
            dw[i] = f.Derivee(parametres[i]);
        }

        int epoch = 0;
        while (epoch < epochs)
        {
            var ys = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                x.Value = points[i].x;
                ys[i] = f.N();
            }

            for (int i = 0; i < parametres.Length; i++)
            {
                double error = 0;
                for (int j = 0; j < points.Length; j++)
                {
                    x.Value = points[j].x;
                    error += (ys[j] - points[j].y) * dw[i].N();
                }
                
                parametres[i].Value -= lr * error / points.Length;
            }
            
            epoch++;
        }


    }
    
}