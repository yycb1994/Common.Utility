using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Utility.Extensions
{
    public static class CalculateExpand
    {
        #region 计算分页总数
        /// <summary>
        /// 计算分页总数
        /// </summary>
        /// <param name="count">数据总数</param>
        /// <param name="pageSize">每页最大显示量</param>
        /// <returns></returns>
        public static long CalculatePageCount(this long count, long pageSize)
        {
            return count % pageSize == 0 ? count / pageSize : count / pageSize + 1;
        }

        /// <summary>
        /// 计算分页总数
        /// </summary>
        /// <param name="count">数据总数</param>
        /// <param name="pageSize">每页最大显示量</param>
        /// <returns></returns>
        public static int CalculatePageCount(this int count, int pageSize)
        {
            return count % pageSize == 0 ? count / pageSize : count / pageSize + 1;
        }
        #endregion


        #region 排序算法

        #region 冒泡排序
        /// <summary>
        /// 冒泡排序：依次比较相邻的两个元素，如果顺序错误则交换。
        /// </summary>
        public static int[] BubbleSort(int[] arr)
        {
            int n = arr.Length;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (arr[j] > arr[j + 1])
                    {
                        int temp = arr[j];
                        arr[j] = arr[j + 1];
                        arr[j + 1] = temp;
                    }
                }
            }
            return arr;
        }
        #endregion

        #region 选择排序
        /// <summary>
        /// 选择排序：每次选择最小的元素放到已排序部分的末尾。
        /// </summary>
        public static int[] SelectionSort(int[] arr)
        {
            int n = arr.Length;
            for (int i = 0; i < n - 1; i++)
            {
                int minIndex = i;
                for (int j = i + 1; j < n; j++)
                {
                    if (arr[j] < arr[minIndex])
                    {
                        minIndex = j;
                    }
                }
                int temp = arr[i];
                arr[i] = arr[minIndex];
                arr[minIndex] = temp;
            }
            return arr;
        }
        #endregion

        #region 插入排序
        /// <summary>
        /// 插入排序：将一个元素插入到已排序部分的合适位置。
        /// </summary>
        public static int[] InsertionSort(int[] arr)
        {
            int n = arr.Length;
            for (int i = 1; i < n; i++)
            {
                int key = arr[i];
                int j = i - 1;
                while (j >= 0 && arr[j] > key)
                {
                    arr[j + 1] = arr[j];
                    j--;
                }
                arr[j + 1] = key;
            }
            return arr;
        }

        #endregion

        #region 归并排序
        /// <summary>
        /// 归并排序：将数组分为两个子数组，分别排序后合并。
        /// </summary>
        public static int[] MergeSort(int[] arr)
        {
            MergeSortRecursive(arr, 0, arr.Length - 1);
            return arr;
        }

        private static void MergeSortRecursive(int[] arr, int left, int right)
        {
            if (left < right)
            {
                int mid = (left + right) / 2;
                MergeSortRecursive(arr, left, mid);
                MergeSortRecursive(arr, mid + 1, right);
                Merge(arr, left, mid, right);
            }

        }

        private static void Merge(int[] arr, int left, int mid, int right)
        {
            int n1 = mid - left + 1;
            int n2 = right - mid;

            int[] L = new int[n1];
            int[] R = new int[n2];

            Array.Copy(arr, left, L, 0, n1);
            Array.Copy(arr, mid + 1, R, 0, n2);

            int i = 0, j = 0, k = left;

            while (i < n1 && j < n2)
            {
                if (L[i] <= R[j])
                {
                    arr[k] = L[i];
                    i++;
                }
                else
                {
                    arr[k] = R[j];
                    j++;
                }
                k++;
            }

            while (i < n1)
            {
                arr[k] = L[i];
                i++;
                k++;
            }

            while (j < n2)
            {
                arr[k] = R[j];
                j++;
                k++;
            }

        }

        #endregion

        #region 快速排序
        /// <summary>
        /// 快速排序：选取一个基准值，将小于基准值的放在左边，大于基准值的放在右边，递归处理左右两部分。
        /// </summary>
        public static int[] QuickSort(int[] arr)
        {
            QuickSortRecursive(arr, 0, arr.Length - 1);
            return arr;
        }

        private static void QuickSortRecursive(int[] arr, int low, int high)
        {
            if (low < high)
            {
                int partitionIndex = Partition(arr, low, high);
                QuickSortRecursive(arr, low, partitionIndex - 1);
                QuickSortRecursive(arr, partitionIndex + 1, high);
            }
        }

        private static int Partition(int[] arr, int low, int high)
        {
            int pivot = arr[high];
            int i = low - 1;

            for (int j = low; j < high; j++)
            {
                if (arr[j] < pivot)
                {
                    i++;
                    int temp = arr[i];
                    arr[i] = arr[j];
                    arr[j] = temp;
                }
            }

            int temp1 = arr[i + 1];
            arr[i + 1] = arr[high];
            arr[high] = temp1;

            return i + 1;
        }

        #endregion

        #region 堆排序
        /// <summary>
        /// 堆排序：将数组构建成最大堆或最小堆，然后逐步取出堆顶元素。
        /// </summary>
        public static int[] HeapSort(int[] arr)
        {
            int n = arr.Length;

            for (int i = n / 2 - 1; i >= 0; i--)
            {
                Heapify(arr, n, i);
            }

            for (int i = n - 1; i > 0; i--)
            {
                int temp = arr[0];
                arr[0] = arr[i];
                arr[i] = temp;

                Heapify(arr, i, 0);
            }
            return arr;
        }

        private static void Heapify(int[] arr, int n, int i)
        {
            int largest = i;
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            if (left < n && arr[left] > arr[largest])
            {
                largest = left;
            }

            if (right < n && arr[right] > arr[largest])
            {
                largest = right;
            }

            if (largest != i)
            {
                int temp = arr[i];
                arr[i] = arr[largest];
                arr[largest] = temp;

                Heapify(arr, n, largest);
            }
        }
        #endregion
        #endregion
    }
}
