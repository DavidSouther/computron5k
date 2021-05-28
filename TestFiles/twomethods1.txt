/* Tests calls to methods with one parameter */

public class twomethods1 {

	public void check (int x)
	{
		Write("The argument to check was ");
		WriteLine(x);
	}
	
	public void main ()
	{
		check(12345);
		WriteLine("Back in main");
	}
}