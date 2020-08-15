// leetcode solutions

// https://leetcode.com/problems/power-of-three/

public class Solution {
    public bool IsPowerOfThree(int n) {        
        if (n > 0) {            
            var maxPowerOf3 = (int) Math.Pow(3, Math.Floor( Math.Log10(Int32.MaxValue) / Math.Log10(3) ));
            return maxPowerOf3 % n == 0;
        }
        return false;
        
    }
}

// https://leetcode.com/problems/contains-duplicate/
// 
// Given an array of integers, find if the array contains any duplicates.
// 
// Your function should return true if any value appears at least twice in the array, and it should return false if every element is distinct.
// 
public class Solution {
    public bool ContainsDuplicate(int[] nums) {
        var dups = new HashSet<int>();        
        foreach(var n in nums){
            if (dups.Contains(n)) {
                return true;
            }
            dups.Add(n);
        }
        return false;
    }
}

// https://leetcode.com/problems/intersection-of-two-arrays-ii/
// 
// Given two arrays, write a function to compute their intersection.
// 
public class Solution {
    public int[] Intersect(int[] nums1, int[] nums2) {
        var small = nums1;
        var large = nums2;
        if (nums2.Length < nums1.Length) {
            small = nums2;
            large = nums1;
        }
        
        var smallDict = new Dictionary<int,int>();
        var result = new List<int>();
        
        foreach(var n in small) {
            if (smallDict.ContainsKey(n)) {
                smallDict[n] = smallDict[n] + 1;
            } else {
                smallDict.Add(n, 1);
            }
        }
        foreach(var n in large) {
            if (smallDict.ContainsKey(n) ) {
                result.Add(n);
                var count = smallDict[n] - 1;                
                if (count == 0){
                    smallDict.Remove(n);
                } else {
                    smallDict[n] = count;
                }
            }
        }
        return result.ToArray();
    }
}

// https://leetcode.com/problems/plus-one/
// 
// Given a non-empty array of digits representing a non-negative integer, increment one to the integer.
// 
// The digits are stored such that the most significant digit is at the head of the list, and each element in the array contains a single digit.
// 
// You may assume the integer does not contain any leading zero, except the number 0 itself.
// 

public class Solution {
    public int[] PlusOne(int[] digits) {
        var carried = true;
        
        for (var i = (digits.Length - 1);i >= 0; i-- ) {
            if (carried) {
                digits[i]++;                        
                if (digits[i] > 9)
                {
                    digits[i] = 0;
                    carried = true;
                } else {
                    carried = false;
                    break;
                }
            }
        }
        
        if (!carried) {
            return digits;
        }
        
        var result = new int[digits.Length + 1];
        Array.Copy(digits, 0, result, 1, digits.Length);
        result[0] = 1;
        return result;
    }
}

// https://leetcode.com/problems/move-zeroes/
// 
// Given an array nums, write a function to move all 0's to the end of it while maintaining the relative order of the non-zero elements.
// 
public class Solution {
    
    public void MoveZeroes(int[] nums) {
        var tgt = 0;
        var tailSize = 0;
        var i = 0;
        while (i < nums.Length) {
            if (nums[i] != 0) {
                nums[tgt] = nums[i];
                i++;
                tgt++;
            } else {
                i++;
                while(i < nums.Length) {
                    // skip zeroes
                    if (nums[i] == 0) {
                        i++;
                    } else {
                        // write tgt
                        nums[tgt] = nums[i];
                        tgt++;
                        i++;
                        break;
                    }
                }
            }
            
        }
        // write remaining zeroes
        for (var x = tgt; x < nums.Length; x++) {
            nums[x] = 0;
        }
    }
}

// https://leetcode.com/problems/two-sum/
// 
// Given an array of integers, return indices of the two numbers such that they add up to a specific target.
// 
// You may assume that each input would have exactly one solution, and you may not use the same element twice.
// 
public class Solution {
    public int[] TwoSum(int[] nums, int target) {
        var map = new Dictionary<int,int>();
        
        var i = 0;
        while ( i < nums.Length) {
            var diff = target - nums[i];
            if (map.ContainsKey(diff)) {
                return new [] {map[diff], i} ;
            } else {
                if (!map.ContainsKey(nums[i])) {
                    map.Add(nums[i], i);    
                }
            }            
            i++;
        }
        return null;
    }
}

// https://leetcode.com/problems/rotate-array/
// 
// Given an array, rotate the array to the right by k steps, where k is non-negative.
// 
// Follow up:
// 
// Try to come up as many solutions as you can, there are at least 3 different ways to solve this problem.
// Could you do it in-place with O(1) extra space?

public class Solution {
    // takes i'th element and writes it to a rotated position
    // returns overwritten element
    public int WriteNumber(int[] nums, int k, int i) {
        var tgtIx = (i + k) % (nums.Length);
        if (tgtIx < i) {
            // was already rotated
        }
        
        var r = nums[tgtIx];
        nums[tgtIx] = nums[i];
        return r;
    }
    
    public void Rotate(int[] nums, int k) {
        var kx = k % nums.Length;
        if (kx == 0) {
            return;
        }
        
        var buffer = new int[kx];
        
        Array.Copy(nums, nums.Length - kx, buffer, 0, kx);
        Array.Copy(nums, 0, nums, kx, nums.Length - kx);
        Array.Copy(buffer, 0, nums, 0, kx);
    }
}

// https://leetcode.com/problems/remove-duplicates-from-sorted-array/
// 
// Given a sorted array nums, remove the duplicates in-place such that each element appear only once and return the new length.
// 
// Do not allocate extra space for another array, you must do this by modifying the input array in-place with O(1) extra memory.
// 
public class Solution {
    public int RemoveDuplicates(int[] nums) {
        if (nums.Length < 2) {
            return nums.Length;
        }
        
        var currentIndex = 0;
        var currentElement = nums[0];
        var nextElementIndex = 1;
        
        while(nextElementIndex < nums.Length) {
            while(nextElementIndex < nums.Length && nums[nextElementIndex] == currentElement) {
                nextElementIndex++;
            }
            
            if (nextElementIndex < nums.Length) {
                currentElement = nums[nextElementIndex];
                currentIndex++;
                nums[currentIndex] = currentElement;
                nextElementIndex++;
            }
        }
        
        return currentIndex + 1;
    }
}


// https://leetcode.com/problems/linked-list-cycle/
// 
// Given a linked list, determine if it has a cycle in it.
// 
// To represent a cycle in the given linked list, we use an integer pos which represents the position (0-indexed) in the linked list where tail connects to. If pos is -1, then there is no cycle in the linked list.
// 
/**
 * Definition for singly-linked list.
 * public class ListNode {
 *     public int val;
 *     public ListNode next;
 *     public ListNode(int x) {
 *         val = x;
 *         next = null;
 *     }
 * }
 */
public class Solution {
    public bool HasCycle(ListNode head) {
        var ix = 0;
        ListNode h = null;
        var i = head;
        if (head == null) {
            return false;
        }
        var icount = 0;
        var hcount = 0 ;
        
        while(i != null) {
            if (i == i.next) {
                return true;
            }
            
            var inext = i.next;
            // inext may now point to node in h which does not contain cycles            
            i.next = h;
            h = i;            
            i = inext;
            icount++;
        }        
        
        while(hcount < icount && h != null) {
            hcount++;
            h = h.next;
        }
        
        return h != null;
    }
}

// https://leetcode.com/problems/palindrome-linked-list/
// 
// Given a singly linked list, determine if it is a palindrome.
// 
/**
 * Definition for singly-linked list.
 * public class ListNode {
 *     public int val;
 *     public ListNode next;
 *     public ListNode(int val=0, ListNode next=null) {
 *         this.val = val;
 *         this.next = next;
 *     }
 * }
 */
public class Solution {
    public bool IsPalindrome(ListNode head) {
        var h = head;
        ListNode r = null;
        
        var count = 0;
        // h = 1 2 3 2 1
        while(h != null) {
            var hnext = h.next;            
            
            // insert into beginning of 'r'
            h.next = r;
            r = h;
            
            h = hnext;
            
            count ++;
        }
        
        // r = 1 2 3 2 1        
                
        var halfCount = count / 2;
        
        for (int i = 0; i < halfCount; i++) {
            // insert into beginning of 'h'            
            var rnext = r.next;
            
            r.next = h;
            h = r;
            
            r = rnext;
        }        
        // r = 3 2 1
        // h = 2 1
        
        if (count % 2 > 0) {
            r = r.next;
        }
        
        // r = 2 1
        // h = 2 1
        for (int i = 0; i < halfCount; i++) {            
            if (r.val != h.val) {
                return false;
            }
            
            r = r.next;
            h = h.next;
        }
        
        return true;
    }
}

// https://leetcode.com/problems/merge-two-sorted-lists/
// 
// Merge two sorted linked lists and return it as a new sorted list. The new list should be made by splicing together the nodes of the first two lists.
// 

/**
 * Definition for singly-linked list.
 * public class ListNode {
 *     public int val;
 *     public ListNode next;
 *     public ListNode(int val=0, ListNode next=null) {
 *         this.val = val;
 *         this.next = next;
 *     }
 * }
 */
public class Solution {
    public ListNode MergeTwoLists(ListNode l1, ListNode l2) {
        
        
        if(l1 == null) {
            return l2;
        } 
        if (l2 == null) {
            return l1;
        }
        
        var target = l1;
        var donor = l2;        
        if (l2.val < l1.val) {
            target = l2;
            donor = l1;
        }
        
                
        var ti = target;
        var di = donor;
        
        while(true) {            
            if (di == null || ti == null) {
                return target;
            }            
            if (ti.next == null) {
                ti.next = di;
                return target;
            } else {
                if (ti.next.val > di.val) {
                    // insert di instead of ti.next
                    // link ti.next after it
                    // advance di to next
                    var tNext = ti.next;
                    ti.next = di;                        
                    di = di.next;
                    ti.next.next = tNext;                        
                } else {
                    ti = ti.next;
                }
            }            
        }
        
        return target;
    }
}

// https://leetcode.com/problems/reverse-linked-list/
// 
// Reverse a singly linked list.

/**
 * Definition for singly-linked list.
 * public class ListNode {
 *     public int val;
 *     public ListNode next;
 *     public ListNode(int val=0, ListNode next=null) {
 *         this.val = val;
 *         this.next = next;
 *     }
 * }
 */
public class Solution {
    
    public ListNode ReverseList(ListNode head) {
        ListNode nh = null;
        var i = head;
        while(i != null) {            
            var n = new ListNode(i.val, nh);
            nh = n;
            i = i.next;
        }
        
        return nh;
    }
}

// https://leetcode.com/problems/remove-nth-node-from-end-of-list/
// 
// Given a linked list, remove the n-th node from the end of list and return its head.
// 
/**
 * Definition for singly-linked list.
 * public class ListNode {
 *     public int val;
 *     public ListNode next;
 *     public ListNode(int val=0, ListNode next=null) {
 *         this.val = val;
 *         this.next = next;
 *     }
 * }
 */
public class Solution {
    public ListNode RemoveNthFromEnd(ListNode head, int n) {
        var inode = head;
        var h = head;
        
        for (int i = 0; i < n; i++) {
            inode = inode.next;
        }
        
        // n = 3
        // 1 2 3 4
        // h
        //       i
        
        if (inode == null) {
            return head.next;
        }
        
        while (inode.next != null ){
            inode = inode.next;
            h = h.next;
        }        
        
        h.next = h.next.next; 
        return head;
        
    }
}

// https://leetcode.com/problems/delete-node-in-a-linked-list/
// 
// Write a function to delete a node (except the tail) in a singly linked list, given only access to that node.
// 


/**
 * Definition for singly-linked list.
 * public class ListNode {
 *     public int val;
 *     public ListNode next;
 *     public ListNode(int x) { val = x; }
 * }
 */
public class Solution {
    public void DeleteNode(ListNode node) {
        node.val = node.next.val;
        node.next = node.next.next;
    }
}

// https://leetcode.com/problems/longest-common-prefix/
// 
// Write a function to find the longest common prefix string amongst an array of strings.
// 
// If there is no common prefix, return an empty string "".
// 
// 
public class Solution {
    public string LongestCommonPrefix(string[] strs) {
        if (strs.Length == 1) {
            return strs[0];
        }
        
        if (strs.Length == 0) {
            return "";
        }
        
        var minLength = strs.Select(x => x.Length).Min();
        var prefixLength = 0;
        for (var i = 0; i < minLength; i++) {
            var c = strs[0][i];
            var matches = true;
            for (var j = 1; j < strs.Length; j++) {
                var c1 = strs[j][i];
                if (c1 != c) {
                    matches = false;
                    break;
                }
            }
            if (matches) {
                prefixLength = i + 1;
            } else {
                break;
            }
        }
        
        if (prefixLength == 0) {
            return "";
        }
        
        return strs[0].Substring(0, prefixLength);        
    }
}
// 
// https://leetcode.com/problems/count-and-say/
// 
// The count-and-say sequence is the sequence of integers with the first five terms as following:
// 
// 1.     1
// 2.     11
// 3.     21
// 4.     1211
// 5.     111221
// 1 is read off as "one 1" or 11.
// 11 is read off as "two 1s" or 21.
// 21 is read off as "one 2, then one 1" or 1211.
// 
// Given an integer n where 1 ≤ n ≤ 30, generate the nth term of the count-and-say sequence. You can do so recursively, in other words from the previous member read off the digits, counting the number of digits in groups of the same digit.
// 
// Note: Each term of the sequence of integers will be represented as a string.
//  
// 
// 
public class Solution {
    private string Say(string s) {
        var count = 1;
        var result = "";
        var c = s[0];
        
        for (int i = 1; i < s.Length; i++) {
            if (s[i] == c) {
                count++;
            } else {
                result = result + (count.ToString()) + (c.ToString());
                c = s[i];
                count = 1;
            }
            
            
        }
        
        result = result + (count.ToString()) + (c.ToString());
        return result;
    }
    
    public string CountAndSay(int n) {
        var it = "1";
        if (n == 1) {
            return it;
        }
        
        for (int i = 1; i < n; i++) {
            it = Say(it);
        }
        
        return it;
        
    }
}

// https://leetcode.com/problems/implement-strstr/
// 
// Implement strStr().
// 
// Return the index of the first occurrence of needle in haystack, or -1 if needle is not part of haystack.
// 
public class Solution {
    public int StrStr(string haystack, string needle) {        
        if (needle.Length == 0){
            return 0;
        }
        
        for (int i = 0; i < haystack.Length; i++){
            var found = true;
            if (i + needle.Length > haystack.Length) {
                return -1;
            }
            
            for (int j = 0; j < needle.Length; j++) {
                if(haystack[i + j] != needle[j]) {
                    found = false;
                    break;
                }
            }
            
            if (found) {
                return i;
            }
        }
        return -1;
    }
}

// https://leetcode.com/problems/string-to-integer-atoi/
// 
// Implement atoi which converts a string to an integer.
// 
// The function first discards as many whitespace characters as necessary until the first non-whitespace character is found. Then, starting from this character, takes an optional initial plus or minus sign followed by as many numerical digits as possible, and interprets them as a numerical value.
// 
// The string can contain additional characters after those that form the integral number, which are ignored and have no effect on the behavior of this function.
// 
// If the first sequence of non-whitespace characters in str is not a valid integral number, or if no such sequence exists because either str is empty or it contains only whitespace characters, no conversion is performed.
// 
// If no valid conversion could be performed, a zero value is returned.
// 
public class Solution {
    public int MyAtoi(string str) {
        var result = 0L;
        var i = 0;
        var isNeg = false;
        // skip whitespaces
        while(i < str.Length) {
            if (str[i] == ' ') {
                i++;
            } else break;
        }
        
        // read sign
        while(i < str.Length) {
            if (str[i] == '+') {
                i++;
                break;
            }
            if (str[i] == '-') {
                i++;
                isNeg = true;
                break;
            }
            break;
        }
        
        // read digits        
        while(i < str.Length) {            
            var ox = str[i] - '0';
            if (ox > -1 && ox < 10) {
                result = result * 10L + ox;
                
                if (!isNeg && result > Int32.MaxValue) {
                    return Int32.MaxValue;
                }
                
                if (isNeg && (0L - result) < Int32.MinValue ) {
                    return Int32.MinValue;
                }
                
                i++;
            } else {
                break;
            }
        }
        
        if (isNeg) {
            result = 0L - result;
        }
        
        return (int)result;        
    }
}

// https://leetcode.com/problems/valid-palindrome/
// 
// Given a string, determine if it is a palindrome, considering only alphanumeric characters and ignoring cases.
// 
public class Solution {
    public bool IsPalindrome(string s) {
        var sanitized = 
            s.ToLowerInvariant().Where(x => (x >= 'a' && x <= 'z') || (x >= '0' && x <= '9')).ToArray();
        
        var len = sanitized.Length / 2;
        
        for (var i = 0; i < len; i++){
            if (sanitized[i] != sanitized[sanitized.Length - (1 + i)]) {
                return false;
            }
        }
        
        return true;
 
    }
}

// https://leetcode.com/problems/valid-anagram/
// 
// Given two strings s and t , write a function to determine if t is an anagram of s.
// 
// 
public class Solution {
    public bool IsAnagram(string s, string t) {
        var nums = new int[26];
        if (s.Length != t.Length) {
             return false;
        }
        
        for (var i = 0; i < s.Length; i++) {
            var ox = s[i] - 'a';
            nums[ox]++;
        }
        
        for (var i = 0; i < t.Length; i++) {
            var ox = t[i] - 'a';
            nums[ox]--;
        }
        
        return nums.All(x => x == 0);
    }
}

// https://leetcode.com/problems/first-unique-character-in-a-string/
// 
// Given a string, find the first non-repeating character in it and return its index. If it doesn't exist, return -1.
// 
public class Solution {
    public int FirstUniqChar(string s) {        
        var nums = new int[26];
        var answers = new List<int>();        
        
        for ( int i = 0; i < s.Length; i++ ) {
            var ox = s[i] - 'a';
            nums[ox]++;
            if (nums[ox] == 1) {
                answers.Add(i);
            }
        }
        
        foreach (var a in answers) {
            var ox = s[a] - 'a';
            if (nums[ox] == 1){
                return a;
            }
        }
        
        return -1;
    }
}

// https://leetcode.com/problems/reverse-integer/
// 
// Given a 32-bit signed integer, reverse digits of an integer.
// 
public class Solution {
    public int Reverse(int x) {        
        var isNegative = x < 0;
        var remaining = isNegative ? -x : x;        
        long result = 0;
        
        while(remaining > 0) {
            var rightMostDigit = remaining % 10;
            result = result * 10L + rightMostDigit;
            remaining = remaining / 10;
        }
        
        if (isNegative) {
            result = 0 - result;
        }
        if (result > Int32.MaxValue || result < Int32.MinValue) {
            return 0;
        }
        
        return (int)result;
        
        
    }
}

// https://leetcode.com/problems/reverse-string/
// 
// Write a function that reverses a string. The input string is given as an array of characters char[].
// 
// Do not allocate extra space for another array, you must do this by modifying the input array in-place with O(1) extra memory.
// 
// You may assume all the characters consist of printable ascii characters.
// 
//  

public class Solution {
    public void ReverseString(char[] s) {
        var i = 0;
        var j = s.Length - 1;
        while (i < j) {
            var c = s[i];
            s[i] = s[j];
            s[j] = c;
            i++;
            j--;
        }
    }
}

// https://leetcode.com/problems/rotate-image/
// 
// You are given an n x n 2D matrix representing an image.
// 
// Rotate the image by 90 degrees (clockwise).
// 
// Note:
// 
// You have to rotate the image in-place, which means you have to modify the input 2D matrix directly. DO NOT allocate another 2D matrix and do the rotation.
// 
// 

public class Solution {
    public void Rotate(int[][] matrix) {
        var n = matrix.Length / 2;        
        var m = matrix.Length - 1;
        // transpose
        for (var i = 0; i < matrix.Length; i++)
        for (var j = i; j < matrix.Length; j++) {
            var c = matrix[i][j];
            matrix[i][j] = matrix[j][i];
            matrix[j][i] = c;
        }
        
        // reverse rows
        for (var i = 0; i < matrix.Length; i++)
        {
            Array.Reverse(matrix[i]);
        }
    }
}

// https://leetcode.com/problems/valid-sudoku/
// 
// Determine if a 9x9 Sudoku board is valid. Only the filled cells need to be validated according to the following rules:
// 
// Each row must contain the digits 1-9 without repetition.
// Each column must contain the digits 1-9 without repetition.
// Each of the 9 3x3 sub-boxes of the grid must contain the digits 1-9 without repetition.
// 
// The Sudoku board could be partially filled, where empty cells are filled with the character '.'.
// 

public class Solution {
    public bool IsValidSudoku(char[][] board) {
        var subBoxes = new Dictionary<Tuple<int,int>,List<char>>();
        for (int i = 0; i < 3; i++) 
        for (int j = 0; j < 3; j++) {
            var ix = Tuple.Create(i,j);
            subBoxes.Add(ix, new List<char>());
        }
        
        for (var i = 0; i < 9; i++){
            var filledRow = board[i].Where(x=>x != '.').ToArray();
            if (filledRow.Length > filledRow.Distinct().Count()) {
                return false;
            }            
        }
        
        
            
        for (int j = 0; j < 9; j++) {
            var filledColumn = new List<char>();
            for (var i = 0; i < 9; i++){
                var c = board[i][j];
                var subBox = Tuple.Create(i / 3, j / 3);
                
                if (c != '.') {
                    subBoxes[subBox].Add(c);
                    filledColumn.Add(c);
                }
            }
            
            if (filledColumn.Count > filledColumn.Distinct().Count()) {
                return false;
            }
        }
        
        foreach (var sb in subBoxes.Values) {
            if (sb.Count > 0) {
                if (sb.Count > sb.Distinct().Count()) {
                    return false;
                }
            }
            
        }
        
        return true;
        
    }
}

// https://leetcode.com/problems/maximum-depth-of-binary-tree/
// 
// Given a binary tree, find its maximum depth.
// 
// The maximum depth is the number of nodes along the longest path from the root node down to the farthest leaf node.
// 
/**
 * Definition for a binary tree node.
 * public class TreeNode {
 *     public int val;
 *     public TreeNode left;
 *     public TreeNode right;
 *     public TreeNode(int val=0, TreeNode left=null, TreeNode right=null) {
 *         this.val = val;
 *         this.left = left;
 *         this.right = right;
 *     }
 * }
 */
public class Solution {
    public int MaxDepth(TreeNode root) {
        if (root == null) {
            return 0;
        }
        
        var left = MaxDepth(root.left);
        var right = MaxDepth(root.right);
        if (left > right) {
            return 1 + left;
        } else {
            return 1 + right;
        }
    }
}

// https://leetcode.com/problems/symmetric-tree/
// 
// Given a binary tree, check whether it is a mirror of itself (ie, symmetric around its center).
// 
/**
 * Definition for a binary tree node.
 * public class TreeNode {
 *     public int val;
 *     public TreeNode left;
 *     public TreeNode right;
 *     public TreeNode(int val=0, TreeNode left=null, TreeNode right=null) {
 *         this.val = val;
 *         this.left = left;
 *         this.right = right;
 *     }
 * }
 */
public class Solution {
    bool isOk(TreeNode left, TreeNode right) {
        if (left == null && right == null) {
            return true;
        }
        
        if (left == null && right != null || left != null && right == null) {
            return false;
        }
        
        if (left.val != right.val) {
            return false;
        }
        
        return isOk(left.left, right.right) && isOk(left.right, right.left);
    }
    
    public bool IsSymmetric(TreeNode root) {
        if (root == null) {
            return true;
        }        
        return isOk(root.left, root.right);
    }
}

// https://leetcode.com/problems/convert-sorted-array-to-binary-search-tree/
// 
// Given an array where elements are sorted in ascending order, convert it to a height balanced BST.
// 
// For this problem, a height-balanced binary tree is defined as a binary tree in which the depth of the two subtrees of every node never differ by more than 1.
// 
/**
 * Definition for a binary tree node.
 * public class TreeNode {
 *     public int val;
 *     public TreeNode left;
 *     public TreeNode right;
 *     public TreeNode(int val=0, TreeNode left=null, TreeNode right=null) {
 *         this.val = val;
 *         this.left = left;
 *         this.right = right;
 *     }
 * }
 */
public class Solution {
    
    TreeNode ToBST(int[] nums, int startIndex, int Length) {
        if (Length == 0) {
            return null;
        }
        
        if (Length == 1) {
            return new TreeNode(nums[startIndex], null, null);
        }
        
        var l = Length / 2;
        var isOdd = Length % 2 > 0;
        var left = ToBST(nums, startIndex, l);
        
        var right = 
            isOdd ? ToBST(nums, startIndex + l + 1, l) : ToBST(nums, startIndex + l + 1, l - 1);
        
        return new TreeNode(nums[startIndex + l], left, right);
    }
    
    public TreeNode SortedArrayToBST(int[] nums) {
        return ToBST(nums, 0, nums.Length);
    }
}

// https://leetcode.com/problems/first-bad-version/
// 
// You are a product manager and currently leading a team to develop a new product. Unfortunately, the latest version of your product fails the quality check. Since each version is developed based on the previous version, all the versions after a bad version are also bad.
// 
// Suppose you have n versions [1, 2, ..., n] and you want to find out the first bad one, which causes all the following ones to be bad.
// 
// You are given an API bool isBadVersion(version) which will return whether version is bad. Implement a function to find the first bad version. You should minimize the number of calls to the API.
// 

/* The isBadVersion API is defined in the parent class VersionControl.
      bool IsBadVersion(int version); */

public class Solution : VersionControl {   
    
    private bool isBad(Dictionary<int, bool> tested, int v){
        if (tested.ContainsKey(v)) {
            return tested[v];
        }
        tested.Add(v, IsBadVersion(v));
        return tested[v];
    }
    
    public int FirstBadVersion(int n) {
        var l = 1;
        var r = n;
        var tested = new Dictionary<int,bool>();
        
        while (l != r) {
            var i = (r - l) / 2;
            if (i == 0) {
                if (isBad(tested, l)) {
                    return l;
                } else {
                    if (isBad(tested, r)){
                        return r;
                    }
                }
            } else {
                var ibad = isBad(tested, l + i);
                if (ibad) {
                    r = l + i;
                } else {
                    l = l + i;
                }
            }            
        }
        
        return l;
    }
}

// https://leetcode.com/problems/climbing-stairs/
// 
// You are climbing a stair case. It takes n steps to reach to the top.
// 
// Each time you can either climb 1 or 2 steps. In how many distinct ways can you climb to the top?
// 
// 

public class Solution {
    public int ClimbStairs(int n) {
        var s = 0;
        var waysToClimb = new Dictionary<int, int>();
        for (int i = 0; i <= n; i++) {
            if ( i == 0) {
                waysToClimb.Add(i, 0);
            }            
            if (i == 1) {
                waysToClimb.Add(i, 1);
            }            
            if (i == 2) {
                waysToClimb.Add(i, 2);
            }
            
            if (i > 2){
                waysToClimb.Add(i, waysToClimb[i - 1] + waysToClimb[i - 2]);
            }
        }
        
        return waysToClimb[n];
    }
}

// https://leetcode.com/problems/best-time-to-buy-and-sell-stock/
// 
// Say you have an array for which the ith element is the price of a given stock on day i.
// 
// If you were only permitted to complete at most one transaction (i.e., buy one and sell one share of the stock), design an algorithm to find the maximum profit.
// 
// Note that you cannot sell a stock before you buy one.
// 
// 

public class Solution {
    public int MaxProfit(int[] prices) {        
        var profit = 0;
        var lastPrice = -1;
        
        for(int i = 0; i < prices.Length; i++){
            if (lastPrice < 0 || prices[i] < lastPrice) {
                lastPrice = prices[i];
            } else {                
                var xprofit = prices[i] - lastPrice;
                if (xprofit > profit) {
                    profit = xprofit;
                }
            }
        }
        
        return profit;
    }
}

// https://leetcode.com/problems/min-stack/
// 
// Design a stack that supports push, pop, top, and retrieving the minimum element in constant time.
// 
// push(x) -- Push element x onto stack.
// pop() -- Removes the element on top of the stack.
// top() -- Get the top element.
// getMin() -- Retrieve the minimum element in the stack.
// 
// 

public class MinStack {
    private List<int> data = new List<int>();
    private List<int> mins = new List<int>();
    private int top = 0;    

    /** initialize your data structure here. */
    public MinStack() {
        
    }
    
    public void Push(int x) {
        if (data.Count > top) {
            data[top] = x;            
        } else {            
            data.Add(x);
        }
        
        var newMin = top > 0 && mins[top - 1] < x ? mins[top - 1] : x;        
        if (mins.Count > top) {
            mins[top] = newMin;            
        } else {
            mins.Add(newMin);
        }
        
        top++;
    }
    
    public void Pop() {
        top--;
    }
    
    public int Top() {
        return data[top - 1];
    }
    
    public int GetMin() {
        return mins[top - 1];
    }
}

/**
 * Your MinStack object will be instantiated and called as such:
 * MinStack obj = new MinStack();
 * obj.Push(x);
 * obj.Pop();
 * int param_3 = obj.Top();
 * int param_4 = obj.GetMin();
 */
 
 
// https://leetcode.com/problems/roman-to-integer/
//  
// Roman numerals are represented by seven different symbols: I, V, X, L, C, D and M.
// 
// Symbol       Value
// I             1
// V             5
// X             10
// L             50
// C             100
// D             500
// M             1000
// For example, two is written as II in Roman numeral, just two one's added together. Twelve is written as, XII, which is simply X + II. The number twenty seven is written as XXVII, which is XX + V + II.
// 
// Roman numerals are usually written largest to smallest from left to right. However, the numeral for four is not IIII. Instead, the number four is written as IV. Because the one is before the five we subtract it making four. The same principle applies to the number nine, which is written as IX. There are six instances where subtraction is used:
// 
// I can be placed before V (5) and X (10) to make 4 and 9. 
// X can be placed before L (50) and C (100) to make 40 and 90. 
// C can be placed before D (500) and M (1000) to make 400 and 900.
//  
// Given a roman numeral, convert it to an integer. Input is guaranteed to be within the range from 1 to 3999.
//  
 
 
 public class Solution {
    public int RomanToInt(string s) {
        Dictionary<char,int> values = new Dictionary<char,int>{
            { 'I', 1 },
            { 'V', 5 },
            { 'X', 10 },            
            { 'L', 50 },
            { 'C', 100 },                        
            { 'D', 500 },
            { 'M', 1000 }            
        };
        
        var v = 0;
        var sv = 0;
        var cp = ' ';
        
        for (int i = 0; i < s.Length; i++) {
            var c = s[i];
            if (i == 0) {
                v = values[c];
            }
            
            if (i > 0 && values[c] > v) {
                v = values[c] - v;
            }
            
            if (i > 0 && values[c] <= values[cp]) {
                sv =  sv + v;
                v = values[c];
            }
            
            if (i == s.Length - 1){
                sv = sv + v;
            }            
            cp = c;
        }
        return sv;
    }
}

// https://leetcode.com/problems/power-of-three/
// 
// Given an integer, write a function to determine if it is a power of three.

public class Solution {
    public bool IsPowerOfThree(int n) {        
        if (n > 0) {            
            var maxPowerOf3 = (int) Math.Pow(3, Math.Floor( Math.Log10(Int32.MaxValue) / Math.Log10(3) ));
            return maxPowerOf3 % n == 0;
        }
        return false;
        
    }
}
