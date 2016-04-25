using System.Linq;

namespace Asyncify.Test.Helpers.Code
{
    public class ProjectUnit
    {
        public SourceCodeUnit[] SupportingSources { get; }

        public ProjectUnit(SourceCodeUnit[] supportingSources)
        {
            SupportingSources = supportingSources;
        }
        
        public string[] TestCodeCompilationUnit(params SourceCodeUnit[] expressions)
        {
            return expressions.Select(s => s.Code).Concat(SupportingSources.Select(s => s.Code)).ToArray();
        }

        public string[] SupportingSourcesAsString()
        {
            return SupportingSources.Select(s => s.Code).ToArray();
        }
    }
}