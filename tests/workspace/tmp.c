
#include <stdlib.h>

void clear() {
	#ifdef _WIN32
	system("cls");
	#elif __linux__
	system("clear");
	#endif
}
