//
// CS1520MethodMustHaveAReturnTypeAction.cs
//
// Author:
//       Luís Reis <luiscubal@gmail.com>
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
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ICSharpCode.NRefactory6.CSharp.Refactoring;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;

namespace ICSharpCode.NRefactory6.CSharp.CodeFixes
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = "Class, struct, or interface method must have a return type"), System.Composition.Shared]
	public class CS1520MethodMustHaveAReturnTypeCodeFixProvider : CodeFixProvider
	{
		const string CS1520 = "CS1520"; // Error CS1520: Method must have a return type

		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create(CS1520); }
		}

		public sealed override async Task RegisterCodeFixesAsync (CodeFixContext context)
		{
			var document = context.Document;
			if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
				return;
			var span = context.Span;
			var cancellationToken = context.CancellationToken;
			if (cancellationToken.IsCancellationRequested)
				return;
			var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			if (model.IsFromGeneratedCode(cancellationToken))
				return;
			var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnostic = context.Diagnostics.First ();
			var syntaxNode = root.FindNode (span);
			var node = syntaxNode as MethodDeclarationSyntax;
			if (node != null && node.Identifier.Span.Contains (span)) {
				context.RegisterFixes (
					new [] {
						CodeActionFactory.Create (
							node.Span,
							DiagnosticSeverity.Error,
							GettextCatalog.GetString ("This is a constructor"),
							t => Task.FromResult (
								document.WithSyntaxRoot (
									root.ReplaceNode (
										(SyntaxNode)node,
										node.WithIdentifier (node.AncestorsAndSelf ().OfType<BaseTypeDeclarationSyntax> ().First ().Identifier.WithoutTrivia ()).WithAdditionalAnnotations (Formatter.Annotation)
									)
								)
							)
						),
						CodeActionFactory.Create (
							node.Span,
							DiagnosticSeverity.Error,
							GettextCatalog.GetString ("This is a void method"),
							t => Task.FromResult (
								document.WithSyntaxRoot (
									root.ReplaceNode (
										(SyntaxNode)node,
										node.WithReturnType (SyntaxFactory.ParseTypeName ("void")).WithAdditionalAnnotations (Formatter.Annotation)
									)
								)
							)
						)
					},
					diagnostic
				);
			}
			var constructor = syntaxNode as MethodDeclarationSyntax;
			if (constructor != null) {
				context.RegisterFixes (
					new [] {
						CodeActionFactory.Create (
							node.Span,
							DiagnosticSeverity.Error,
							GettextCatalog.GetString ("Fix constructor"),
							t => Task.FromResult (
								document.WithSyntaxRoot (
									root.ReplaceNode (
										(SyntaxNode)node,
										node.WithIdentifier (node.AncestorsAndSelf ().OfType<BaseTypeDeclarationSyntax> ().First ().Identifier.WithoutTrivia ()).WithAdditionalAnnotations (Formatter.Annotation)
									)
								)
							)
						)
					},
					diagnostic
				);
			}


		}
//		public async Task ComputeRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
//		{
//			var entity = context.GetNode<ConstructorDeclaration>();
//			if (entity == null)
//				yield break;
//			var type = entity.Parent as TypeDeclaration;
//
//			if (type == null || entity.Name == type.Name)
//				yield break;
//
//			var typeDeclaration = entity.GetParent<TypeDeclaration>();
//
		//			yield return new CodeAction(context.TranslateString("This is a constructor"), script => script.Replace(entity.NameToken, Identifier.Create(typeDeclaration.Name, TextLocation.Empty)), entity) {
//				Severity = ICSharpCode.NRefactory.Refactoring.Severity.Error
//			};
//
//			yield return new CodeAction(context.TranslateString("This is a void method"), script => {
//				var generatedMethod = new MethodDeclaration();
//				generatedMethod.Modifiers = entity.Modifiers;
//				generatedMethod.ReturnType = new PrimitiveType("void");
//				generatedMethod.Name = entity.Name;
//				generatedMethod.Parameters.AddRange(entity.Parameters.Select(parameter => (ParameterDeclaration)parameter.Clone()));
//				generatedMethod.Body = (BlockStatement)entity.Body.Clone();
//				generatedMethod.Attributes.AddRange(entity.Attributes.Select(attribute => (AttributeSection)attribute.Clone()));
//
//				script.Replace(entity, generatedMethod);
//			}, entity) {
//				Severity = ICSharpCode.NRefactory.Refactoring.Severity.Error
//			};
//		}
	}
}
