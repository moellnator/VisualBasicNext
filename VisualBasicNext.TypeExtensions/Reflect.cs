using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Code extracted from Github MedallionUtilities by madelson
/// https://github.com/madelson/MedallionUtilities/tree/master/MedallionReflection
/// Published under MIT License:
/// https://github.com/madelson/MedallionUtilities/blob/master/License.txt
/// </summary>

namespace VisualBasicNext.TypeExtensions
{
    public static partial class Reflect
    {
       
        public static MethodInfo GetMethod(Expression<Action> methodCallExpression)
        {
            LambdaExpression lambda = methodCallExpression;
            return GetMethod(lambda);
        }

        public static MethodInfo GetMethod<TInstance>(Expression<Action<TInstance>> methodCallExpression)
        {
            LambdaExpression lambda = methodCallExpression;
            return GetMethod(lambda);
        }

        private static MethodInfo GetMethod(LambdaExpression methodCallExpression)
        {
            if (methodCallExpression == null) { throw new ArgumentNullException(nameof(methodCallExpression)); }
            if (!(methodCallExpression.Body is MethodCallExpression methodCall))
            {
                throw new ArgumentException("methodCallExpression: the body of the lambda expression must be a method call. Found: " + methodCallExpression.Body.NodeType);
            }

            return methodCall.Method;
        }
        
    }
}
