FROM python:3.14-slim

RUN useradd -m apiuser \
  && mkdir /home/apiuser/src \
  && chown apiuser /home/apiuser/src

COPY --chown=apiuser --chmod=755 requirements.txt /home/apiuser/requirements.txt

# install pip packages
RUN pip install --upgrade -r /home/apiuser/requirements.txt \
  && pip install gunicorn

COPY --chown=apiuser --chmod=755 src/ /home/apiuser/src/

WORKDIR /home/apiuser

USER apiuser

ENV log_level="debug"

ENTRYPOINT [ "gunicorn", "-w", "1", "src/main.py", "--config", "./config.json" ]
