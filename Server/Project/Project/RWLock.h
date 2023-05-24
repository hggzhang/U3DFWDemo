#pragma once
#pragma warning(disable: 26110)
#include <mutex>
#include <shared_mutex>
using std::mutex;
#include <thread>
using std::condition_variable;

#define RALL_RWLOCK_READ_LOCK(_lock) std::shared_lock<std::shared_mutex> lock(_lock)
#define RALL_RWLOCK_WRITE_LOCK(_lock) std::unique_lock<std::shared_mutex> lock(_lock)
#define RALL_RWLOCK mutable std::shared_mutex



//using std::unique_lock;
//class RWLock
//{
//public:
//    RWLock() :
//        mWaitReadThreadNum(0),
//        mWaitWriteTrheadNum(0),
//        mReadingThreadNum(0),
//        mWritingThreadNum(0) {
//    }
//
//    ~RWLock() {};
//
//    void read_lock() {
//        unique_lock<mutex> uniLock(mMyMutex);
//        if (mWritingThreadNum || mWaitWriteTrheadNum) { //写优先，只要有线程在等待写，则不能让读线程得到机会。
//            ++mWaitReadThreadNum;
//            while (mWritingThreadNum || mWaitWriteTrheadNum) {
//                mReadThreadCV.wait(uniLock);
//            }
//            --mWaitReadThreadNum;
//        }
//        ++mReadingThreadNum;
//    }
//
//    void write_lock() {
//        unique_lock<mutex> uniLock(mMyMutex);
//        if (mWritingThreadNum || mReadingThreadNum) {
//            ++mWaitWriteTrheadNum;
//            while (mWritingThreadNum || mReadingThreadNum) {
//                mWriteThreadCV.wait(uniLock);
//            }
//            --mWaitWriteTrheadNum;
//        }
//        ++mWritingThreadNum;
//    }
//
//    void read_unlock() {
//        unique_lock<mutex> uniLock(mMyMutex);
//        --mReadingThreadNum;
//        if (mWaitWriteTrheadNum) {//有写线程在等待的话，直接尝试唤醒一个写线程，即使还有其他线程在读。写优先！
//
//            mWriteThreadCV.notify_one();
//
//        }
//    }
//
//    void write_unlock() {
//        unique_lock<mutex> uniLock(mMyMutex);
//        --mWritingThreadNum;
//        if (mWaitWriteTrheadNum) {//写优先
//            mWriteThreadCV.notify_one();
//        }
//        else {
//            mReadThreadCV.notify_all();//通知所有被阻塞的read线程
//        }
//    }
//
//private:
//    int mWaitReadThreadNum, mReadingThreadNum;
//
//    int mWaitWriteTrheadNum, mWritingThreadNum;
//
//    mutex mMyMutex;
//    condition_variable mReadThreadCV;//用于“读线程”的等待和唤醒。
//    condition_variable mWriteThreadCV;//用于"写线程"的等待和唤醒
//};
