document.querySelectorAll('.drop-zone').forEach(zone=>{
 const input=zone.querySelector('input[type=file]'),preview=zone.querySelector('.previews');let urls=[];
 function render(){
 urls.forEach(URL.revokeObjectURL);urls=[];preview.replaceChildren();input.setCustomValidity('');
 if(input.files.length>(input.multiple?10:1)){input.setCustomValidity('En fazla '+(input.multiple?'10':'1')+' fotoğraf seçin.');input.reportValidity();return;}
 Array.from(input.files).forEach((file,index)=>{
 if(!['image/jpeg','image/png','image/webp'].includes(file.type)||file.size>10*1024*1024){input.setCustomValidity('PNG, JPEG veya WebP; en fazla 10 MB.');input.reportValidity();return;}
 const item=document.createElement('div');item.className='preview-item';
 const img=document.createElement('img');img.alt=file.name;img.src=URL.createObjectURL(file);urls.push(img.src);item.append(img);
 const remove=document.createElement('button');remove.type='button';remove.className='preview-remove';remove.textContent='×';remove.setAttribute('aria-label',file.name+' fotoğrafını kaldır');
 remove.addEventListener('click',()=>{const dt=new DataTransfer();Array.from(input.files).forEach((f,i)=>{if(i!==index)dt.items.add(f)});input.files=dt.files;render();input.focus();});item.append(remove);preview.append(item);
 });
 }
 input.addEventListener('change',render);
 zone.addEventListener('dragover',e=>{e.preventDefault();zone.classList.add('dragging')});
 zone.addEventListener('dragleave',()=>zone.classList.remove('dragging'));
 zone.addEventListener('drop',e=>{e.preventDefault();zone.classList.remove('dragging');const dt=new DataTransfer();Array.from(e.dataTransfer.files).forEach(f=>dt.items.add(f));if(!input.multiple&&dt.files.length>1){input.setCustomValidity('Tek fotoğraf seçin.');input.reportValidity();return;}input.files=dt.files;render();});
});
function syncLogo(){const input=document.getElementById('Logo'),section=document.getElementById('logo-upload');if(!input||!section)return;const use=document.querySelector('[name=UseSavedLogo]:checked')?.value==='true';section.hidden=use;input.required=!use;}
document.querySelectorAll('[name=UseSavedLogo]').forEach(r=>r.addEventListener('change',syncLogo));syncLogo();
const form=document.getElementById('catalog-form');
if(form){const min=document.getElementById('MinimumAge'),max=document.getElementById('MaximumAge'),unit=document.getElementById('AgeUnit');function age(){max.max=unit.value==='Years'?18:216;min.max=max.max;max.setCustomValidity(+min.value>+max.value?'Minimum yaş maksimum yaştan büyük olamaz.':'');document.getElementById('age-preview').textContent='Yaklaşık manken yaşı: '+((+min.value + +max.value)/2)+' '+(unit.value==='Years'?'yaş':'ay');}[min,max,unit].forEach(x=>x.addEventListener('input',age));age();form.addEventListener('submit',()=>{document.getElementById('generate').disabled=true;document.getElementById('generate').textContent='Katalog hazırlanıyor…';document.getElementById('progress').hidden=false;});}
document.querySelectorAll('form[data-confirm]').forEach(f=>f.addEventListener('submit',e=>{if(!confirm(f.dataset.confirm))e.preventDefault();}));
document.querySelectorAll('form[data-generation]').forEach(form=>form.addEventListener('submit',event=>{if(event.defaultPrevented)return;const button=form.querySelector('button');if(button){button.disabled=true;button.textContent='Yeniden oluşturuluyor…';}}));
document.addEventListener('click',event=>{document.querySelectorAll('.account-menu[open]').forEach(menu=>{if(!menu.contains(event.target))menu.open=false;});});
document.addEventListener('keydown',event=>{if(event.key==='Escape')document.querySelectorAll('.account-menu[open]').forEach(menu=>{menu.open=false;menu.querySelector('summary').focus();});});
window.addEventListener('pageshow',event=>{if(event.persisted){const button=document.getElementById('generate');if(button){button.disabled=false;button.textContent='Kataloğu oluştur · 200 token';document.getElementById('progress').hidden=true;}document.querySelectorAll('form[data-generation] button').forEach(b=>{b.disabled=false;b.textContent='Yeniden oluştur · 50 token';});}});
