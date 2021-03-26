#include <string>
#include <iostream>
#include <filesystem>

extern "C" void c_ls(const char* p)
{
  std::string path = p;
  for (const auto & entry : std::filesystem::directory_iterator(path))
  {
    auto x = entry.path();
    std::cout << x.filename().filename().string() << std::endl;
  }
}