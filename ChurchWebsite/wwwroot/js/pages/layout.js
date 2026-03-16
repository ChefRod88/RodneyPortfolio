(function(){
  var c=document.getElementById('globalConstellation');
  if(!c)return;
  var ctx=c.getContext('2d'),W,H,pts,maxDist=130,raf;
  function resize(){
    W=c.width=window.innerWidth;
    H=c.height=window.innerHeight;
  }
  function initPts(){
    pts=Array.from({length:60},function(){
      return{x:Math.random()*W,y:Math.random()*H,vx:(Math.random()-.5)*.18,vy:(Math.random()-.5)*.18,r:Math.random()*1.2+.25,a:Math.random()*.4+.08};
    });
  }
  function draw(){
    ctx.clearRect(0,0,W,H);
    for(var i=0;i<pts.length;i++){
      for(var j=i+1;j<pts.length;j++){
        var dx=pts[i].x-pts[j].x,dy=pts[i].y-pts[j].y,d=Math.sqrt(dx*dx+dy*dy);
        if(d<maxDist){
          ctx.beginPath();
          ctx.moveTo(pts[i].x,pts[i].y);
          ctx.lineTo(pts[j].x,pts[j].y);
          ctx.strokeStyle='rgba(201,168,76,'+(0.15*(1-d/maxDist))+')';
          ctx.lineWidth=.5;
          ctx.stroke();
        }
      }
    }
    pts.forEach(function(p){
      p.x+=p.vx;p.y+=p.vy;
      if(p.x<0)p.x=W;if(p.x>W)p.x=0;
      if(p.y<0)p.y=H;if(p.y>H)p.y=0;
      ctx.beginPath();
      ctx.arc(p.x,p.y,p.r,0,Math.PI*2);
      ctx.fillStyle='rgba(201,168,76,'+p.a+')';
      ctx.fill();
    });
    raf=requestAnimationFrame(draw);
  }
  resize();initPts();draw();
  window.addEventListener('resize',function(){resize();initPts()});
  /* Pause when tab hidden to save CPU */
  document.addEventListener('visibilitychange',function(){
    if(document.hidden){cancelAnimationFrame(raf)}else{draw()}
  });
})();
