#include <iostream>
#include <stop_token>
#include <string>
#include <string_view>
#include <thread>
#include "ipc/postbox.hpp"

int main(int argc, char* argv[]) {
  using namespace std::literals;
  if (argc != 2) {
    std::cout << "Usage: tincan-client <zmq-socket-uri>\n";
    return 1;
  }
  tc::postbox postbox(argv[1]);

  std::jthread postbox_loop([&](const std::stop_token& token) {
    while (true) {
      postbox.event_loop(token);
      if (token.stop_requested()) {
        return;
      }
    }
  });

  std::string line;
  while (true) {
    std::getline(std::cin, line);
    if (line == "exit\n"sv) {
      break;
    }
    
    size_t split = line.find_first_of(' ');
    if (split == std::string::npos)
      split = line.size();

    postbox.enqueue(
      std::string_view(line.begin(), line.begin() + split),
      std::string_view(line.begin() + split, line.end()));
  }
  
  postbox_loop.request_stop();
  if (postbox_loop.joinable())
    postbox_loop.join();
}