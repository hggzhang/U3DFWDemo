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
//        if (mWritingThreadNum || mWaitWriteTrheadNum) { //д���ȣ�ֻҪ���߳��ڵȴ�д�������ö��̵߳õ����ᡣ
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
//        if (mWaitWriteTrheadNum) {//��д�߳��ڵȴ��Ļ���ֱ�ӳ��Ի���һ��д�̣߳���ʹ���������߳��ڶ���д���ȣ�
//
//            mWriteThreadCV.notify_one();
//
//        }
//    }
//
//    void write_unlock() {
//        unique_lock<mutex> uniLock(mMyMutex);
//        --mWritingThreadNum;
//        if (mWaitWriteTrheadNum) {//д����
//            mWriteThreadCV.notify_one();
//        }
//        else {
//            mReadThreadCV.notify_all();//֪ͨ���б�������read�߳�
//        }
//    }
//
//private:
//    int mWaitReadThreadNum, mReadingThreadNum;
//
//    int mWaitWriteTrheadNum, mWritingThreadNum;
//
//    mutex mMyMutex;
//    condition_variable mReadThreadCV;//���ڡ����̡߳��ĵȴ��ͻ��ѡ�
//    condition_variable mWriteThreadCV;//����"д�߳�"�ĵȴ��ͻ���
//};
