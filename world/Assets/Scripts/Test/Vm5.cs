using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Vm5 : TestBase
{
    public struct Stack
    {
        public int value;
        public object ov; 
    }

    public class Context
    {
        public readonly Stack[] ints = new Stack[10000];
        public readonly Transform[] objects = new Transform[1000];
        public readonly Vector3[] vector3s = new Vector3[100];
        public int esp_int;
        public int esp_v3;
        public int esp_object;
    }

    public readonly static Context ctx = new Context();

    public abstract class Statement
    {
        public abstract void Run();
    }

    public class Set_StackInt_ConstInt : Statement
    {
        public int dst;
        public int value;
        public override void Run()
        {
            ctx.ints[ctx.esp_int + dst].value = value;
        }
    }

    public class Set_StackInt_StackInt : Statement
    {
        public int dst;
        public int src;
        public override void Run()
        {
            ctx.ints[ctx.esp_int + dst].value = ctx.ints[ctx.esp_int + src].value;
        }
    }

    public class Set_StackTransform_Position : Statement
    {
        public int src;
        public int dst;
        public override void Run()
        {
            (ctx.objects[ctx.esp_object + dst] as Transform).position = ctx.vector3s[ctx.esp_v3 + dst];
        }
    }


    public class AddAssign_StackInt_StackInt : Statement
    {
        public int dst;
        public int src;

        public override void Run()
        {
            ctx.ints[ctx.esp_int + dst].value += ctx.ints[ctx.esp_int + src].value;
        }
    }

    public class AddAssign_StackInt_ConstInt : Statement
    {
        public int dst;
        public int value;

        public override void Run()
        {
            ctx.ints[ctx.esp_int + dst].value += value;
        }
    }

    public class Add_StackInt_StackInt_StackInt : Statement
    {
        public int dst;
        public int src1;
        public int src2;

        public override void Run()
        {
            ctx.ints[ctx.esp_int + dst].value = ctx.ints[ctx.esp_int + src1].value + ctx.ints[ctx.esp_int + src2].value;
        }
    }

    public abstract class BoolExpr
    {
        public abstract bool eval();
    }

    public abstract class IntExpr
    {
        public abstract int eval();
    }

    public class IE_Const : IntExpr
    {
        public int value;
        public override int eval()
        {
            return value;
        }
    }

    public class IE_Stack : IntExpr
    {
        public int src;

        public override unsafe int eval()
        {
            return ctx.ints[ctx.esp_int + src].value;
        }
    }

    public class BE_IntLessEqual : BoolExpr
    {
        public IntExpr left;
        public IntExpr right;

        public override bool eval()
        {
            return left.eval() <= right.eval();
        }
    }



    public class Loop : Statement
    {
        public BoolExpr condition;
        public Statement[] stats;

        public override void Run()
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

    public abstract class Expr_GameObject : Statement
    {
        public abstract GameObject eval();
        public override void Run()
        {
            eval();
        }
    }

    public class EG_new : Expr_GameObject
    {
        public override GameObject eval()
        {
            return new GameObject();
            //GameObject.DestroyImmediate(x);
            //return null;
        }
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
        foreach(var st in sts)
        {
            st.Run(); 
        }
    }

    public override void test2()
    {
        var sts = new List<Statement>()
        {
            new Set_StackInt_ConstInt {dst = 1, value = 0 },
            new Loop
            {
                condition = new BE_IntLessEqual { left = new IE_Stack {src = 1}, right = new IE_Const {value = N2 } },
                stats = new Statement[]
                {
                    new AddAssign_StackInt_ConstInt {dst = 1, value = 1 },
                    new EG_new(),
                }
            }
        };
        foreach (var st in sts)
        {
            st.Run();
        }
    }

    public override void test3()
    {
        ctx.objects[0] = new GameObject().transform;
        ctx.vector3s[0] = new Vector3(1, 2, 3);
        var sts = new List<Statement>()
        {
            new Set_StackInt_ConstInt {dst = 1, value = 0 },
            new Loop
            {
                condition = new BE_IntLessEqual { left = new IE_Stack {src = 1}, right = new IE_Const {value = N3 } },
                stats = new Statement[]
                {
                    new AddAssign_StackInt_ConstInt {dst = 1, value = 1 },
                    new Set_StackTransform_Position {dst = 0, src = 0 },
                    new Set_StackTransform_Position {dst = 0, src = 0 },
                    new Set_StackTransform_Position {dst = 0, src = 0 },
                    new Set_StackTransform_Position {dst = 0, src = 0 },
                    new Set_StackTransform_Position {dst = 0, src = 0 },
                    new Set_StackTransform_Position {dst = 0, src = 0 },
                    new Set_StackTransform_Position {dst = 0, src = 0 },
                    new Set_StackTransform_Position {dst = 0, src = 0 },
                    new Set_StackTransform_Position {dst = 0, src = 0 },
                    new Set_StackTransform_Position {dst = 0, src = 0 },
                }
            }
        };
        foreach (var st in sts)
        {
            st.Run();
        }
    }

    public override void test4()
    {
        
    }
}

