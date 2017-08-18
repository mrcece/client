using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class Test
{
    

    public void GetX(out int y)
    {
        y = x;
    }
    private int x;

    class RefField
    {
        public Test obj;
        public int value;
        public RefField(Test obj)
        {
            this.obj = obj;
            this.value = obj.x;
        }
        public void Update()
        {
            obj.x = value;
        }
    }

    public void test()
    {
        /*
        RefField rf = new RefField(this);
        GetX(out rf.value);
        rf.Update();

        new StarndCs().run();
        new Vm1().run();
        new Vm2().run();
        new Vm3().run();
        //new Vm4().run();
        new Vm5().run();
        */
    }
}

