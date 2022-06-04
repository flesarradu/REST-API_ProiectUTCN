using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json;
using System.IO;
using REST_API_ProiectUTCN.Models;

namespace REST_API_ProiectUTCN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly MessageContext _context;


        public MessagesController(MessageContext context)
        {
            _context = context;

            if (!context.Messages.Any())
            {
                var x = GetMessagesJson();
                x.Wait();
            }

        }

        private async Task GetMessagesJson()
        {
            try
            {
                var filepath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "json");
                var d = new DirectoryInfo(filepath);

                foreach (var file in d.GetFiles("*.json"))
                {
                    var file1 = new FileStream(file.FullName, FileMode.Open);
                    var x = await System.Text.Json.JsonSerializer.DeserializeAsync<IEnumerable<Message>>(file1);
                    foreach (var message in x)
                    {
                        if (!MessageExists(message.Id))
                            await _context.Messages.AddAsync(message);
                    }

                    file1.Close();
                }

                await _context.SaveChangesAsync();
            }
            catch{
                Console.WriteLine("error");
            }
        }

        // GET: api/Messages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessages()
        {
            return await this._context.Messages.ToListAsync();
        }

        // GET: api/Messages/paul/5
        [HttpGet("{user}/{id}")]
        public async Task<IEnumerable<Message>> GetMessage(string user, long id)
        {
            return await _context.Messages.Where(x => (x.Id == id && x.User == user)).ToListAsync();
            
        }
        // GET: api/Messages/paul
        [HttpGet("{user}")]
        public async Task<IEnumerable<Message>> GetMessage(string user)
        {
            return await _context.Messages.Where(x=>x.User == user).ToListAsync();
        }

        // PUT: api/Messages/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMessage(long id, Message message)
        {
            if (id != message.Id)
            {
                return BadRequest();
            }

            _context.Entry(message).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MessageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Messages
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Message>> PostMessage(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            //Save in json file
            string jsonString = JsonConvert.SerializeObject(message);
            if (jsonString != "")
            {
                string fileName = $"{System.IO.Directory.GetCurrentDirectory()}\\json\\{message.User + ".json"}";

                var file1 = new FileStream(fileName, FileMode.Open);
                jsonString = JsonConvert.SerializeObject(await _context.Messages.ToListAsync());
                await System.IO.File.WriteAllTextAsync(fileName, jsonString);
                file1.Close();
            }
            
            
            

            return CreatedAtAction("GetMessage", new { id = message.Id }, message);
        }

        // DELETE: api/Messages/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(long id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MessageExists(long id)
        {
            return _context.Messages.Any(e => e.Id == id);
        }
    }
}
