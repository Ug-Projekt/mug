#include <fstream>
#include <iostream>
#include <string>

extern "C" const char* $char_readfile_const_char(const char* path) 
{ 
  std::ifstream file(path);

  if (file.fail())
  {
    return 0;
  }

  std::string result;
  std::string str;

  while (std::getline(file, str))
  {
    result += str; result.append("\n");
  }

  return result.c_str();
}