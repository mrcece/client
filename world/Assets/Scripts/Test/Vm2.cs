using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Vm2 : TestBase
{
    public unsafe class Context
    {
        public int* ints;
        public int esp_int;
    }

    private readonly static Context ctx = new Context();

    abstract class Statement
    {
        public abstract unsafe void Run();
    }

    class Set_StackInt_ConstInt : Statement
    {
        public int dst;
        public int value;
        public unsafe override void Run()
        {
            ctx.ints[ctx.esp_int + dst] = value;
        }
    }


    class AddAssign_StackInt_StackInt : Statement
    {
        public int dst;
        public int src;

        public unsafe override void Run()
        {
            ctx.ints[ctx.esp_int + dst] += ctx.ints[ctx.esp_int + src];
        }
    }

    class AddAssign_StackInt_ConstInt : Statement
    {
        public int dst;
        public int value;

        public unsafe override void Run()
        {
            ctx.ints[ctx.esp_int + dst] += value;
        }
    }

    class Add_StackInt_StackInt_StackInt : Statement
    {
        public int dst;
        public int src1;
        public int src2;

        public override unsafe void Run()
        {
            ctx.ints[ctx.esp_int + dst] = ctx.ints[ctx.esp_int + src1] + ctx.ints[ctx.esp_int + src2];
        }
    }

    abstract class BoolExpr
    {
        public abstract bool eval();
    }

    abstract class IntExpr
    {
        public abstract int eval();
    }

    class IE_Const : IntExpr
    {
        public int value;
        public override int eval()
        {
            return value;
        }
    }

    class IE_Stack : IntExpr
    {
        public int src;

        public override unsafe int eval()
        {
                return ctx.ints[ctx.esp_int + src];
        }
    }

    class BE_IntLessEqual : BoolExpr
    {
        public IntExpr left;
        public IntExpr right;

        public override bool eval()
        {
            return left.eval() <= right.eval();
        }
    }



    class Loop : Statement
    {
        public BoolExpr condition;
        public Statement[] stats;

        public unsafe override void Run()
        {
            while(condition.eval())
            {
                foreach(var s in stats)
                {
                    s.Run();
                }
            }
        }
    }



    public override unsafe void test1() 
    {
        var sts = new List<Statement>()
        {
            new Set_StackInt_ConstInt { dst = 0, value = 0},
            new Set_StackInt_ConstInt {dst = 1, value = 0 },
            new Loop
            {
                condition = new BE_IntLessEqual { left = new IE_Stack {src = 1}, right = new IE_Const {value = N1 } },
                stats = new Statement[]
                {
                    new AddAssign_StackInt_StackInt {dst = 0, src = 1},
                    new AddAssign_StackInt_ConstInt {dst = 1, value = 1 },

                    new Add_StackInt_StackInt_StackInt {dst = 2, src1 = 1, src2 = 2 },
                    new Add_StackInt_StackInt_StackInt {dst = 3, src1 = 2, src2 = 3},
                    new Add_StackInt_StackInt_StackInt {dst = 4, src1 = 3, src2 = 2},
                    new Add_StackInt_StackInt_StackInt {dst = 4, src1 = 3, src2 = 2},
                    new Add_StackInt_StackInt_StackInt {dst = 4, src1 = 3, src2 = 2},
                    new Add_StackInt_StackInt_StackInt {dst = 4, src1 = 3, src2 = 2},
                    new Add_StackInt_StackInt_StackInt {dst = 4, src1 = 3, src2 = 2},
                    new Add_StackInt_StackInt_StackInt {dst = 4, src1 = 3, src2 = 2},
                    new Add_StackInt_StackInt_StackInt {dst = 4, src1 = 3, src2 = 2},
                    new Add_StackInt_StackInt_StackInt {dst = 4, src1 = 3, src2 = 2},
                }
            }
        };

        int[] inits = new int[1000];
       fixed(int*p = inits)
        {
            ctx.ints = p;
            foreach (var st in sts)
            {
                st.Run();
            }
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

