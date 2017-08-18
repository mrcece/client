using System;
using System.Collections.Generic;


public class Vm3 : Vm1
{

    class For : Statement
    {
        public int src;
        public int start;
        public int end;
        public Statement[] stats;

        public override void Run()
        {
            ctx.ints[ctx.esp_int + src] = start;
            while (ctx.ints[src + ctx.esp_int] < end)
            {
                foreach (var s in stats)
                {
                    s.Run();
                }
            }
        }
    }

    public class Add_StackInt_StackInt : IntExpr
    {
        public int src1;
        public int src2;
        public override int eval()
        {
            return ctx.ints[src1 + ctx.esp_int] + ctx.ints[src2 + ctx.esp_int];
        }
    }

    public class Set_StackInt_Expr : Statement
    {
        public int dst;
        public IntExpr expr;

        public override void Run()
        {
            ctx.ints[ctx.esp_int + dst] = expr.eval();
        }
    }


    public override void test1()
    {
        var sts = new List<Statement>()
        {
            new Set_StackInt_ConstInt { dst = 0, value = 0},
            new Set_StackInt_ConstInt {dst = 1, value = 0 },
            new For
            {
                src = 1, start = 0, end = N1,
                stats = new Statement[]
                {
                    new AddAssign_StackInt_StackInt {dst = 0, src = 1},
                    new AddAssign_StackInt_ConstInt {dst = 1, value = 1 },

                    new Set_StackInt_Expr { dst = 4, expr = new Add_StackInt_StackInt {src1 = 2, src2 =3} },
                    new Set_StackInt_Expr { dst = 4, expr = new Add_StackInt_StackInt {src1 = 2, src2 =3} },
                    new Set_StackInt_Expr { dst = 4, expr = new Add_StackInt_StackInt {src1 = 2, src2 =3} },
                    new Set_StackInt_Expr { dst = 4, expr = new Add_StackInt_StackInt {src1 = 2, src2 =3} },
                    new Set_StackInt_Expr { dst = 4, expr = new Add_StackInt_StackInt {src1 = 2, src2 =3} },
                    new Set_StackInt_Expr { dst = 4, expr = new Add_StackInt_StackInt {src1 = 2, src2 =3} },
                    new Set_StackInt_Expr { dst = 4, expr = new Add_StackInt_StackInt {src1 = 2, src2 =3} },
                    new Set_StackInt_Expr { dst = 4, expr = new Add_StackInt_StackInt {src1 = 2, src2 =3} },
                    new Set_StackInt_Expr { dst = 4, expr = new Add_StackInt_StackInt {src1 = 2, src2 =3} },
                    new Set_StackInt_Expr { dst = 4, expr = new Add_StackInt_StackInt {src1 = 2, src2 =3} },
                }
            }
        };
        foreach (var st in sts)
        {
            st.Run();
        }
    }

    public override void test2()
    {

    }

    public override void test3()
    {

    }

    public override void test4()
    {

    }
}



