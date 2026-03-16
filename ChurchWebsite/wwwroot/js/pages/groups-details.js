function openJoinModal(){var m=document.getElementById('joinModal');if(m){m.hidden=false;document.body.style.overflow='hidden'}}
function closeJoinModal(){var m=document.getElementById('joinModal');if(m){m.hidden=true;document.body.style.overflow=''}}
document.addEventListener('keydown',function(e){if(e.key==='Escape')closeJoinModal()});
(function(){var obs=new IntersectionObserver(function(e){e.forEach(function(x){if(x.isIntersecting)x.target.classList.add('visible')})},{threshold:.1});document.querySelectorAll('.sr').forEach(function(el){obs.observe(el)})})();
