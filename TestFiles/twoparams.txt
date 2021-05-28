/* Tests calls to methods with two parameters */
/* A qualified name is used to refer to the method */

public class twoparams {

   public static void outTwoInts(int p1, int p2) {
     Write (p1);
     WriteLine (" is the value of the first parameter");
     Write (p2);
     WriteLine (" is the value of the second parameter");
   }     
/* */
   public static void main() {
     int w,x,y;
     WriteLine ("TCCL test of method with two parameters test");
     y = 3+4;  
     x = 5*7;  
     twoparams.outTwoInts (y, x);
   }
}