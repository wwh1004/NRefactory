//
// AddBracesActionTests.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc. (http://xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using NUnit.Framework;
using ICSharpCode.NRefactory6.CSharp.Refactoring;

namespace ICSharpCode.NRefactory6.CSharp.CodeRefactorings
{
	[TestFixture]
	public class AddBracesTests : ContextActionTestBase
	{
		[Test]
		public void TestAddBracesToIf()
		{
			Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $if (true)
            Console.WriteLine(""Hello"");
    }
}", @"class TestClass
{
    void Test()
    {
        if (true)
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
		}

		[Test]
		public void TestAddBracesToIfWithComment()
		{
			Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        // Some comment
        $if (true)
            Console.WriteLine(""Hello"");
    }
}", @"class TestClass
{
    void Test()
    {
        // Some comment
        if (true)
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
		}

		[Ignore("broken")]
		[Test]
		public void TestAddBracesToIfWithCommentInBlock()
		{
			Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $if (true)
            // Some comment
            Console.WriteLine(""Hello"");
    }
}", @"class TestClass
{
    void Test()
    {
        if (true)
        {
            // Some comment
            Console.WriteLine(""Hello"");
        }
    }
}");
		}

		[Test]
		public void TestAddBracesToElse()
		{
			Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        if (true)
        {
            Console.WriteLine(""Hello"");
        } $else
            Console.WriteLine(""World"");
    }
}", @"class TestClass
{
    void Test()
    {
        if (true)
        {
            Console.WriteLine(""Hello"");
        } else
        {
            Console.WriteLine(""World"");
        }
    }
}");
		}

		[Test]
		public void TestAddBracesToDoWhile()
		{
			Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $do
            Console.WriteLine(""Hello""); 
        while (true);
    }
}", @"class TestClass
{
    void Test()
    {
        do
        {
            Console.WriteLine(""Hello"");
        }
        while (true);
    }
}");
		}

		[Test]
		public void TestAddBracesToForeach()
		{
			Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $foreach (var a in b)
            Console.WriteLine(""Hello"");
    }
}", @"class TestClass
{
    void Test()
    {
        foreach (var a in b)
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
		}

		[Test]
		public void TestAddBracesToFor()
		{
			Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $for (;;)
            Console.WriteLine(""Hello"");
    }
}", @"class TestClass
{
    void Test()
    {
        for (;;)
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
		}

		[Test]
		public void TestAddBracesToLock()
		{
			Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $lock (this)
            Console.WriteLine(""Hello"");
    }
}", @"class TestClass
{
    void Test()
    {
        lock (this)
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
		}

		[Test]
		public void TestAddBracesToUsing()
		{
			Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $using (var a = new A ())
            Console.WriteLine(""Hello"");
    }
}", @"class TestClass
{
    void Test()
    {
        using (var a = new A ())
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
		}

		[Test]
		public void TestAddBracesToWhile()
		{
			Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $while (true)
            Console.WriteLine(""Hello"");
    }
}", @"class TestClass
{
    void Test()
    {
        while (true)
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
		}

		[Test]
		public void TestBlockAlreadyInserted()
		{
			TestWrongContext<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $if (true)
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
		}

		[Test]
		public void TestNullNode()
		{
			TestWrongContext<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        if (true)
        {
            Console.WriteLine(""Hello"");
        }
    }
}

        $          
");
		}
	}
}
