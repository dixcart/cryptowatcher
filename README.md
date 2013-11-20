
#CryptoWatcher

An application that creates "honeypot" documents and watches them for changes.  If the files are modified, then the `LanmanServer` service is stopped and no more corruption of shares can take place.

##Instructions

  * Compile and copy exe to server you want to protect.
  * Run program once to generate a sample XML file.
  * Modify XML file to point to the local paths of your share(s).  Multiple `folder` nodes are allowed.
  * Create a scheduled task to run application at startup, make sure "start in" is set to same directory as exe is located.  Also make sure the user you run the task as has rights to the local folders and can start/stop services.

The application inserts a reference Word doc in each listed folder and creates a `FileSystemWatcher` on each one.  As soon as a modification **of any type** is detected, the application stops the `LanmanServer` service, which stops all file sharing on that server, and emails the specified address that this has happened.

##License

<a rel="license" href="http://creativecommons.org/licenses/by-sa/3.0/"><img alt="Creative Commons Licence" style="border-width:0" src="http://i.creativecommons.org/l/by-sa/3.0/88x31.png" /></a><br /><span xmlns:dct="http://purl.org/dc/terms/" href="http://purl.org/dc/dcmitype/InteractiveResource" property="dct:title" rel="dct:type">CryptoWatcher</span> by <a xmlns:cc="http://creativecommons.org/ns#" href="http://dixcart.com/it" property="cc:attributionName" rel="cc:attributionURL">Dixcart Technical Solutions Limited</a> is licensed under a <a rel="license" href="http://creativecommons.org/licenses/by-sa/3.0/">Creative Commons Attribution-ShareAlike 3.0 Unported License</a>.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.