#include <time.h>
#include <stdlib.h>

void c_void_init_rand() {
  srand(time(NULL));
}

int c_int_rand(int min, int max) {
  return (rand() % (max - min + 1)) + min;
}