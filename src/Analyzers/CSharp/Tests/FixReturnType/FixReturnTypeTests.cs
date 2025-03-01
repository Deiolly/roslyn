﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.CodeFixes.FixReturnType;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Diagnostics.FixReturnType
{
    [Trait(Traits.Feature, Traits.Features.CodeActionsFixReturnType)]
    public partial class FixReturnTypeTests : AbstractCSharpDiagnosticProviderBasedUserDiagnosticTest
    {
        public FixReturnTypeTests(ITestOutputHelper logger)
             : base(logger)
        {
        }

        internal override (DiagnosticAnalyzer, CodeFixProvider) CreateDiagnosticProviderAndFixer(Workspace workspace)
            => (null, new CSharpFixReturnTypeCodeFixProvider());

        [Fact]
        public async Task Simple()
        {
            await TestInRegularAndScript1Async(
@"class C
{
    void M()
    {
        [|return|] 1;
    }
}",
@"class C
{
    int M()
    {
        return 1;
    }
}");
        }

        [Fact]
        public async Task Simple_WithTrivia()
        {
            await TestInRegularAndScript1Async(
@"class C
{
    /*A*/ void /*B*/ M()
    {
        [|return|] 1;
    }
}",
@"class C
{
    /*A*/
    int /*B*/ M()
    {
        return 1;
    }
}");
            // Note: the formatting change is introduced by Formatter.FormatAsync
        }

        [Fact]
        public async Task ReturnString()
        {
            await TestInRegularAndScript1Async(
@"class C
{
    void M()
    {
        [|return|] """";
    }
}",
@"class C
{
    string M()
    {
        return """";
    }
}");
        }

        [Fact]
        public async Task ReturnNull()
        {
            await TestInRegularAndScript1Async(
@"class C
{
    void M()
    {
        [|return|] null;
    }
}",
@"class C
{
    object M()
    {
        return null;
    }
}");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/33481")]
        public async Task ReturnTypelessTuple()
        {
            await TestInRegularAndScript1Async(
@"class C
{
    void M()
    {
        [|return|] (null, string.Empty);
    }
}",
@"class C
{
    object M()
    {
        return (null, string.Empty);
    }
}");
        }

        [Fact]
        public async Task ReturnLambda()
        {
            await TestInRegularAndScript1Async(
@"class C
{
    void M()
    {
        [|return|] () => {};
    }
}",
 @"class C
{
    object M()
    {
        return () => {};
    }
}");
        }

        [Fact]
        public async Task ReturnC()
        {
            await TestInRegularAndScript1Async(
@"class C
{
    void M()
    {
        [|return|] new C();
    }
}",
@"class C
{
    C M()
    {
        return new C();
    }
}");
        }

        [Fact]
        public async Task ReturnString_AsyncVoid()
        {
            await TestInRegularAndScript1Async(
@"class C
{
    async void M()
    {
        [|return|] """";
    }
}",
@"class C
{
    async System.Threading.Tasks.Task<string> M()
    {
        return """";
    }
}");
        }

        [Fact]
        public async Task ReturnString_AsyncVoid_WithUsing()
        {
            await TestInRegularAndScript1Async(
@"
using System.Threading.Tasks;
class C
{
    async void M()
    {
        [|return|] """";
    }
}",
@"
using System.Threading.Tasks;
class C
{
    async Task<string> M()
    {
        return """";
    }
}");
        }

        [Fact]
        public async Task ReturnString_AsyncTask()
        {
            await TestInRegularAndScript1Async(
@"class C
{
    async System.Threading.Tasks.Task M()
    {
        [|return|] """";
    }
}",
@"class C
{
    async System.Threading.Tasks.Task<string> M()
    {
        return """";
    }
}");
        }

        [Fact]
        public async Task ReturnString_LocalFunction()
        {
            await TestInRegularAndScript1Async(
@"class C
{
    void M()
    {
        void local()
        {
            [|return|] """";
        }
    }
}",
@"class C
{
    void M()
    {
        string local()
        {
            return """";
        }
    }
}");
        }

        [Fact]
        public async Task ReturnString_AsyncVoid_LocalFunction()
        {
            await TestInRegularAndScript1Async(
@"class C
{
    void M()
    {
        async void local()
        {
            [|return|] """";
        }
    }
}",
@"class C
{
    void M()
    {
        async System.Threading.Tasks.Task<string> local()
        {
            return """";
        }
    }
}");
        }

        [Fact]
        public async Task ExpressionBodied()
        {
            await TestInRegularAndScript1Async(
@"class C
{
    void M() => 1[||];
}",
@"class C
{
    int M() => 1[||];
}");
        }

        [Fact]
        [WorkItem(47089, "https://github.com/dotnet/roslyn/issues/47089")]
        public async Task ExpressionAndReturnTypeAreVoid()
        {
            await TestMissingInRegularAndScriptAsync(
@"class C
{
    void M()
    {
        return Console.WriteLine()[||];
    }
}");
        }

        [Fact]
        [WorkItem(53574, "https://github.com/dotnet/roslyn/issues/53574")]
        public async Task TestAnonymousTypeTopLevel()
        {
            await TestInRegularAndScript1Async(
@"class C
{
    public void Method()
    {
        [|return|] new { A = 0, B = 1 };
    }
}",
@"class C
{
    public object Method()
    {
        return new { A = 0, B = 1 };
    }
}");
        }

        [Fact]
        [WorkItem(53574, "https://github.com/dotnet/roslyn/issues/53574")]
        public async Task TestAnonymousTypeTopNested()
        {
            await TestInRegularAndScript1Async(
@"class C
{
    public void Method()
    {
        [|return|] new[] { new { A = 0, B = 1 } };
    }
}",
@"class C
{
    public object Method()
    {
        return new[] { new { A = 0, B = 1 } };
    }
}");
        }
    }
}
