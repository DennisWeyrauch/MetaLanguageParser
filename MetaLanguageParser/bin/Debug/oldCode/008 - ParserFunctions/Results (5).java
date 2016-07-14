import java.util.*;
public class Program {
	public static void main(String[] args) {
		int i;
		int j = 0;
		if ((i < 15) & (i > 0)) {
			while (i < 15) {
				i = i + 1;
				System.out.print(i);
			}
		} else {
			do {
				i = i - 1;
				System.out.print(i);
			} while (i > 0);
		}
	}
	

}
