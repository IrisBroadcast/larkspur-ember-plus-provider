/*
   EmberLib.net -- .NET implementation of the Ember+ Protocol

   Copyright (C) 2012-2019 Lawo GmbH (http://www.lawo.com).
   Distributed under the Boost Software License, Version 1.0.
   (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
*/
// XXX: Changes has been made, I Added license header, Also maybe should see if something is listening to these events.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EmberLib
{
   /// <summary>
   /// Singleton that provides a global event raised when a validation error occurs on decoding a node.
   /// </summary>
   public class DecoderValidation
   {
      #region Singleton
      DecoderValidation()
      {
      }

      public static readonly DecoderValidation Instance = new DecoderValidation();
      #endregion

      #region ValidationError Event
      public class ValidationErrorArgs : EventArgs
      {
         public EmberNode Node { get; private set; }
         public string Message { get; private set; }

         public ValidationErrorArgs(EmberNode node, string message)
         {
            Node = node;
            Message = message;
         }
      }

      /// <summary>
      /// Raised when a validation error occurs on decoding a node.
      /// </summary>
      public event EventHandler<ValidationErrorArgs> ValidationError;

      protected virtual void OnValidationError(ValidationErrorArgs e)
      {
          Debug.WriteLine($"Error: DecoderValidation / OnValidationError", e);
          if (ValidationError != null)
            ValidationError(this, e);
      }
      #endregion

      internal bool HasSubscriptions
      {
         get { return ValidationError != null; }
      }

      internal void RaiseValidationError(EmberNode node, string message)
      {
         OnValidationError(new ValidationErrorArgs(node, message));
      }
   }
}
