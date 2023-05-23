#ifndef TC_IPC_SAFE_QUEUE_HPP
#define TC_IPC_SAFE_QUEUE_HPP

#include <condition_variable>
#include <deque>
#include <memory>
#include <mutex>
#include <queue>

namespace tc {
  template <class T, class Container = std::deque<T>>
  class safe_queue : public std::queue<T, Container> {
  public:
    using container_type  = Container;
    using value_type      = typename Container::value_type;
    using size_type       = typename Container::size_type;
    using reference       = typename Container::reference;
    using const_reference = typename Container::const_reference;

    using std::queue<T, Container>::queue;

    reference front() {
      std::unique_lock lock(m_mutex);
      m_cv.wait(lock, [this] { return !std::queue<T, Container>::empty(); });
      return std::queue<T, Container>::front();
    }

    reference back() {
      std::unique_lock lock(m_mutex);
      m_cv.wait(lock, [this] { return !std::queue<T, Container>::empty(); });
      return std::queue<T, Container>::back();
    }

    using std::queue<T, Container>::empty;
    using std::queue<T, Container>::size;

    void push(const value_type& value) {
      {
        std::lock_guard lock(m_mutex);
        std::queue<T, Container>::push(value);
      }
      m_cv.notify_one();
    }

    template <class... Args>
    decltype(auto) emplace(Args&&... args) {
      {
        std::lock_guard lock(m_mutex);
        std::queue<T, Container>::emplace(std::forward<Args>(args)...);
      }
      m_cv.notify_one();
    }

    void pop() {
      std::lock_guard lock(m_mutex);
      std::queue<T, Container>::pop();
    }
    
    void pop_return(reference ref) {
      std::lock_guard lock(m_mutex);
      ref = std::move(std::queue<T, Container>::front());
      std::queue<T, Container>::pop();
    }

    void swap(safe_queue& rhs) {
      // swapping with self does nothing
      if (this == std::addressof(rhs))
        return;
      // lock both mutexes and swap
      std::scoped_lock lock(m_mutex, rhs.m_mutex);
      std::queue<T, Container>::swap(rhs);
    }

  private:
    std::mutex m_mutex {};
    std::condition_variable m_cv {};
  };
  
  template <class T, class Container>
  void swap(safe_queue<T, Container>& a, safe_queue<T, Container>& b) {
    a.swap(b);
  }
}  // namespace tc

#endif