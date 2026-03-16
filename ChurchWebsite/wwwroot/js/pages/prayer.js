(function(){
  var canvas=document.getElementById('prCanvas'),ctx=canvas.getContext('2d'),W,H;
  function resize(){W=canvas.width=window.innerWidth;H=canvas.height=window.innerHeight}
  resize();window.addEventListener('resize',resize);
  var orbs=Array.from({length:4},function(_,i){return{x:Math.random()*1200,y:Math.random()*800,r:120+i*100,vx:(Math.random()-.5)*.25,vy:(Math.random()-.5)*.25,a:.05+i*.008}});
  (function loop(){ctx.clearRect(0,0,W,H);orbs.forEach(function(o){o.x+=o.vx;o.y+=o.vy;if(o.x<-o.r)o.x=W+o.r;if(o.x>W+o.r)o.x=-o.r;if(o.y<-o.r)o.y=H+o.r;if(o.y>H+o.r)o.y=-o.r;var g=ctx.createRadialGradient(o.x,o.y,0,o.x,o.y,o.r);g.addColorStop(0,'rgba(201,168,76,'+o.a+')');g.addColorStop(1,'rgba(201,168,76,0)');ctx.beginPath();ctx.arc(o.x,o.y,o.r,0,Math.PI*2);ctx.fillStyle=g;ctx.fill()});requestAnimationFrame(loop)})();
  var textarea=document.getElementById('prayerText');
  document.querySelectorAll('.pr-cat').forEach(function(chip){chip.addEventListener('click',function(){document.querySelectorAll('.pr-cat').forEach(function(c){c.classList.remove('active')});this.classList.add('active');if(textarea){textarea.placeholder=this.dataset.ph||'Share your heart...';textarea.focus()}})});
  if(textarea){textarea.addEventListener('input',function(){var el=document.getElementById('charCount');if(el)el.textContent=this.value.length})}
  var form=document.getElementById('prayerForm');
  if(form){form.addEventListener('submit',function(){var btn=document.getElementById('prayerSubmit'),txt=document.getElementById('prayerBtnText');if(btn)btn.classList.add('loading');if(txt)txt.textContent='Submitting...'})}
  document.querySelectorAll('.sr').forEach(function(el){new IntersectionObserver(function(e){e.forEach(function(x){if(x.isIntersecting)x.target.classList.add('visible')})},{threshold:.08}).observe(el)});
})();
