import os
import subprocess
import time
import urllib.request
import xml.etree.ElementTree as ET

def generate_static_site():
    print("Starting ASP.NET Core server...")
    process = subprocess.Popen(
        ["dotnet", "run", "--urls=http://localhost:5000", "--environment=Production"],
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE
    )

    # Wait for the server to be ready
    server_ready = False
    for i in range(30):
        try:
            urllib.request.urlopen("http://localhost:5000/")
            server_ready = True
            break
        except Exception:
            time.sleep(1)

    if not server_ready:
        print("Error: Server did not start in time.")
        process.kill()
        exit(1)

    print("Server is ready. Fetching sitemap...")
    
    wwwroot_dir = os.path.join(os.getcwd(), "wwwroot")
    os.makedirs(wwwroot_dir, exist_ok=True)

    try:
        sitemap_req = urllib.request.urlopen("http://localhost:5000/sitemap.xml")
        sitemap_xml = sitemap_req.read()
        
        # Save sitemap.xml
        with open(os.path.join(wwwroot_dir, "sitemap.xml"), "wb") as f:
            f.write(sitemap_xml)
        
        root = ET.fromstring(sitemap_xml)
        namespace = {"ns": "http://www.sitemaps.org/schemas/sitemap/0.9"}
        urls = [elem.text for elem in root.findall(".//ns:loc", namespace)]
        
        # Add feed.xml and robots.txt explicitly since they are not in the standard sitemap locs
        urls.append("https://www.rodneyachery.com/robots.txt")
        urls.append("https://www.rodneyachery.com/Articles/feed.xml")

        for url in urls:
            path = url.replace("https://www.rodneyachery.com", "")
            if not path or path == "/":
                local_path = "index.html"
                fetch_url = "http://localhost:5000/"
            else:
                # Remove leading slash
                path_trimmed = path.lstrip("/")
                
                if path_trimmed.endswith(".xml") or path_trimmed.endswith(".txt"):
                    local_path = path_trimmed
                else:
                    local_path = os.path.join(path_trimmed, "index.html")
                    
                fetch_url = f"http://localhost:5000/{path_trimmed}"
            
            save_path = os.path.join(wwwroot_dir, local_path)
            os.makedirs(os.path.dirname(save_path), exist_ok=True)
            
            print(f"Fetching {fetch_url} -> {save_path}")
            try:
                response = urllib.request.urlopen(fetch_url)
                content = response.read()
                with open(save_path, "wb") as f:
                    f.write(content)
            except Exception as e:
                print(f"Failed to fetch {fetch_url}: {e}")

    finally:
        print("Stopping server...")
        process.kill()
        process.wait()
        print("Static site generation complete.")

if __name__ == "__main__":
    generate_static_site()
