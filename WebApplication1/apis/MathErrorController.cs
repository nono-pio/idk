//
//
// using Microsoft.AspNetCore.Mvc;
// using System.Collections.Generic;
// using Microsoft.AspNetCore.Http.HttpResults;
//
// namespace WebApplication1.apis;
//
// [ApiController]
// [Route("api/[controller]")]
// public class MathErrorController
// {
//
//     private static List<MathError> Errors = new();
//
//     // GET: api/items - Renvoie tous les objets de la liste
//     [HttpGet]
//     public ActionResult<List<MathError>> GetMathErrors()
//     {
//         return Ok(Errors);
//     }
//
//     // POST: api/items - Ajoute un nouvel objet à la liste
//     [HttpPost]
//     public ActionResult AddMathError([FromBody] MathError newMathError)
//     {
//         if (newMathError == null)
//         {
//             return BadRequest("L'objet ne peut pas être nul");
//         }
//
//         Errors.Add(newMathError);
//         return CreatedAtAction(nameof(GetMathErrors), new { id = newMathError.Id }, newMathError);
//     }
// }
//
//
// public record MathError();