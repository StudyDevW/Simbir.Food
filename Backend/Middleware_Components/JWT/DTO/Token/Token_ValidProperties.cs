using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware_Components.JWT.DTO.Token
{
    public class Token_ValidProperties
    {
        public Token_ValidSuccess? token_success { get; set; }

        public Token_ValidError? token_error { get; set; }

        public bool TokenHasError() { return token_error != null; }

        public bool TokenHasSuccess() { return token_success != null; }
    }
}
