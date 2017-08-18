using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Vm4 : TestBase
{
    public class Context
    {
        public int[] ints = new int[10000];
        public int esp_int;
    }

    public readonly static Context ctx = new Context();

    public class Statement
    {
        public readonly int type;
        public Statement(int type)
        {
            this.type = type;
        }
    }

    public class Set_StackInt_ConstInt : Statement
    {
        public Set_StackInt_ConstInt() : base(0)
        {
            
        }
        public int dst;
        public int value;
    }

    public class Set_StackInt_StackInt : Statement
    {
        public int dst;
        public int src;
        public Set_StackInt_StackInt() : base(1) { }
    }


    public class AddAssign_StackInt_StackInt : Statement
    {
        public int dst;
        public int src;
        public AddAssign_StackInt_StackInt() : base(2) { }
    }

    public class AddAssign_StackInt_ConstInt : Statement
    {
        public int dst;
        public int value;
        public AddAssign_StackInt_ConstInt() : base(3) { }
    }

    public class Add_StackInt_StackInt_StackInt : Statement
    {
        public int dst;
        public int src1;
        public int src2;
        public Add_StackInt_StackInt_StackInt() : base(4) { }
    }

    public class BoolExpr
    {
        public readonly int type;
        public BoolExpr(int type)
        {
            this.type = type;
        }
    }

    public class IntExpr
    {
        public readonly int type;
        public IntExpr(int type)
        {
            this.type = type;
        }
    }

    public class IE_Const : IntExpr
    {
        public int value;
        public IE_Const() : base(0) { }
    }

    public class IE_Stack : IntExpr
    {
        public int src;
        public IE_Stack() : base(1) { }
    }

    public class BE_IntLessEqual : BoolExpr
    {
        public IntExpr left;
        public IntExpr right;
        public BE_IntLessEqual() : base(0) { }
    }



    public class Loop : Statement
    {
        public BoolExpr condition;
        public Statement[] stats;

        public Loop() : base(5) { }
    }


    public override void test1()
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
        foreach (var st in sts)
        {
            execute(st);
        }
    }

    private static void execute(Statement s)
    {
        switch(s.type)
        {
            case 0:
                {
                    var t = s as Set_StackInt_ConstInt;
                    ctx.ints[ctx.esp_int + t.dst] = t.value;
                    break;
                }
            case 1:
                {
                    var t = s as Set_StackInt_StackInt;
                    ctx.ints[ctx.esp_int + t.dst] = ctx.ints[ctx.esp_int + t.src];
                    break;
                }
            case 2:
                {
                    var t = s as AddAssign_StackInt_StackInt;
                    ctx.ints[ctx.esp_int + t.dst] += ctx.ints[ctx.esp_int + t.src];
                    break;
                }
            case 3:
                {
                    var t = s as AddAssign_StackInt_ConstInt;
                    ctx.ints[ctx.esp_int + t.dst] += t.value;
                    break;
                }

            case 4:
                {
                    var t = s as Add_StackInt_StackInt_StackInt;
                    ctx.ints[ctx.esp_int + t.dst] = ctx.ints[ctx.esp_int + t.src1] + ctx.ints[ctx.esp_int + t.src2];
                    break;
                }
            case 5:
                {
                    var t = s as Loop;
                    while(eval_bool(t.condition))
                    {
                        foreach(var st in t.stats)
                        {
                            execute(st);
                        }
                    }
                    break;
                }
            default: {
                    throw new Exception("unknown type;" + s.type);
                }
        }
    }

    private static int eval_int(IntExpr e)
    {
        switch (e.type)
        {
            case 0:
                {
                    var t = e as IE_Const;
                    return t.value;
                }
            case 1:
                {
                    var t = e as IE_Stack;
                    return ctx.ints[ctx.esp_int + t.src];
                }
            default:
                {
                    throw new Exception("unknown type:" + e.type);
                }
        }
    }

    private static bool eval_bool(BoolExpr e)
    {
        switch (e.type)
        {
            case 0:
                {
                    var t = e as BE_IntLessEqual;
                    return eval_int(t.left) <= eval_int(t.right);
                }
            default:
                {
                    throw new Exception("unknown type:" + e.type);
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

